using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;
using StackExchange.Redis;
using Order = AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Order;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Membership;

sealed partial class UserMembershipService(
    ILogger<UserMembershipService> logger,
    IConnectionMultiplexer connection,
    IUserMembershipRepository userMembershipRepo,
    //IUserMembershipChangeRecordRepository userMembershipChangeRecordRepo,
    IMembershipBusinessOrderRepository membershipBusinessOrderRepo,
    IMembershipGoodsRepository membershipGoodsRepo,
    //IMembershipProductKeyRecordRepository membershipProductKeyRecordRepo,
    IMerchantDeductionAgreementRepository agreementRepo) : IUserMembershipService
{
    public async Task<string?> CreateMembershipOrderAsync(
        Guid userId,
        MembershipGoods goods,
        Guid? channelPackageId,
        string? orderId = null,
        CancellationToken cancellationToken = default)
    {
        // 检查是否使用过首次优惠，使用过则按商品当前正常价格计算
        decimal amountReceivable;
        var useFirstPrice = await membershipGoodsRepo.CheckUserUseFirstPriceOfGoodsAsync(
            userId, goods.Id, cancellationToken);

        amountReceivable = !useFirstPrice && goods.FirstCurrentPrice.HasValue
            ? goods.FirstCurrentPrice.Value
            : goods.CurrentPrice;

        var membershipOrder = new MembershipBusinessOrder
        {
            RechargeDays = goods.RechargeDays,
            AmountReceivable = amountReceivable,
            UserId = userId,
            Note = "购买会员",
            GoodsNo = goods.GoodsNo,
            GoodsName = goods.GoodsName,
            MemberLicenseType = goods.MemberLicenseType,
            MembershipGoodsId = goods.Id,
            BusinessSource = MembershipBusinessSource.普通订单,
            ChannelPackageId = channelPackageId,
        };

        var result = await membershipBusinessOrderRepo.CreateBusinessOrder(membershipOrder, orderId: orderId);
        return result.Success ? result.Order?.Id : null;
    }

    public async Task<bool> CreateMembershipOrderByCDKeyAsync(
        Guid userId,
        MembershipProductKeyRecord productKeyRecord,
        MembershipGoods goods,
        CancellationToken cancellationToken = default)
    {
        if (productKeyRecord.MembershipGoodsId != goods.Id)
        {
            return false;
        }

        var membershipOrder = new MembershipBusinessOrder
        {
            RechargeDays = productKeyRecord.RechargeDays,
            UserId = userId,
            Note = "CDKey 兑换会员",
            GoodsNo = goods.GoodsNo,
            GoodsName = goods.GoodsName,
            MemberLicenseType = goods.MemberLicenseType,
            MembershipGoodsId = goods.Id,
            BusinessSource = MembershipBusinessSource.CDK激活,
            ProductKeyRecordId = productKeyRecord.Id,
        };

        var result = await membershipBusinessOrderRepo.CreateBusinessOrder(membershipOrder);
        return result.Success;
    }

    #region SubscribeHandle / 支付订单通知处理

    public async Task<bool> OrderPaymentSuccessHandleAsync(string orderId)
    {
        (var isSuccess, var userId) = await membershipBusinessOrderRepo.OrderPaymentSuccess(orderId);

        if (isSuccess) await RefreshUserMembershipCacheAsync(userId!.Value);

        return isSuccess;
    }

    public async Task<bool> OrderPaymentRefundedHandleAsync(string orderId)
    {
        (var isSuccess, var userId) = await membershipBusinessOrderRepo.OrderRefunded(orderId);

        if (isSuccess) await RefreshUserMembershipCacheAsync(userId!.Value);

        return isSuccess;
    }

    public Task<bool> OrderPaymentCancelHandleAsync(string orderId)
    {
        // 会员充值成功后可以通过退款成功后撤销会员，所以这里不需要终止业务
        return Task.FromResult(true);
    }

    #endregion

    #region MerchantDeduction / 商户扣款（连续订阅处理）

    public async Task<Order?> CreateMembershipOrderByMerchantDeductionAsync(MerchantDeductionAgreement agreement)
    {
        var agreement_business_order = await membershipBusinessOrderRepo.NoTrackingFirstOrDefaultAsync(x => x.MerchantDeductionAgreementId == agreement.Id);
        if (agreement_business_order is null)
            return null;

        var membershipOrder = new MembershipBusinessOrder
        {
            RechargeDays = GetRechargeDays(agreement.PeriodType, agreement.Period),
            AmountReceivable = agreement.SingleAmount,
            UserId = agreement.UserId,
            Note = agreement.Note,
            MerchantDeductionAgreementId = agreement.Id,
            PaymentStatus = OrderStatus.WaitPay,
            GoodsRechargeStatus = GoodsRechargeStatus.Waiting,
            GoodsNo = agreement_business_order.GoodsNo,
            GoodsName = agreement_business_order.GoodsName,
            MemberLicenseType = agreement_business_order.MemberLicenseType,
            MembershipGoodsId = agreement_business_order.MembershipGoodsId,
            BusinessSource = MembershipBusinessSource.协议扣款,
        };

        // 创建瓦特会员订单：通用订单、业务订单、支付组成一并创建
        var result = await membershipBusinessOrderRepo.CreateBusinessOrder(
            membershipOrder,
            true,
            agreement.Platform);
        if (!result.Success || result.Order is null)
        {
            logger.LogError("签约扣款 创建会员订单错误");
            return null;
        }
        return result.Order;

        static int GetRechargeDays(string periodType, int period) => periodType switch
        {
            "MONTH" => 30 * period,
            "DAY" => period,
            _ => 0
        };
    }

    public async Task<bool> SignMerchantDeductionSuccessHandleAsync(string agreementNo)
    {
        var agreement = await agreementRepo.GetAgreementAndOrdersByNo(agreementNo);

        if (agreement is null)
            return false;

        var business_order = await membershipBusinessOrderRepo.GetBusinessOrderByAgreementAsync(agreement.Id);
        if (business_order is null)
            return false;

        var handleSuccess = await userMembershipRepo.AddUserMembershipFlagAsync(business_order.UserId, business_order.MemberLicenseType);

        if (handleSuccess) await RefreshUserMembershipCacheAsync(business_order.UserId);

        return handleSuccess;
    }

    public async Task<bool> UnSignMerchantDeductionSuccessHandleAsync(string agreementNo)
    {
        var agreement = await agreementRepo.GetAgreementAndOrdersByNo(agreementNo);

        if (agreement is null)
            return false;

        var business_order = await membershipBusinessOrderRepo.GetBusinessOrderByAgreementAsync(agreement.Id);
        if (business_order is null)
            return false;

        var handleSuccess = await userMembershipRepo.RemoveUserMembershipFlagAndCheckExpiredAsync(business_order.UserId, business_order.MemberLicenseType);

        if (handleSuccess) await RefreshUserMembershipCacheAsync(business_order.UserId);

        return handleSuccess;
    }

    #endregion

    public async Task<bool> RefreshUserMembershipCacheAsync(Guid userId)
    {
        var database = connection.GetDatabase(CacheKeys.RedisMessagingDb);
        var cacheKey = CacheKeys.GetUserMembershipCacheKey(userId);
        await database.KeyDeleteAsync(cacheKey);

        {
            // 刷新 UserInfoModel
            var database2 = connection.GetDatabase(CacheKeys.RedisHashDataDb);
            var hashKey = ShortGuid.Encode(userId);
            await database2.KeyDeleteAsync($"{CacheKeys.IdentityUserInfoDataHashV1Key}:{hashKey}");
        }

        return true;
    }

    public async Task<bool> EditUserMembershipWithRefreshUserMembershipCacheAsync(
        Guid userId,
        Guid? bmUserId,
        DateTimeOffset? endTime,
        TimeSpan? timeSpan,
        string? note)
    {
        if (!endTime.HasValue && !timeSpan.HasValue)
        {
            return false;
        }

        var rowCount = await userMembershipRepo.EditUserMembershipAsync(userId, bmUserId, endTime, timeSpan, note);
        var isOK = rowCount > 0;
        if (isOK)
        {
            isOK = await RefreshUserMembershipCacheAsync(userId);
            return isOK;
        }
        return false;
    }
}
