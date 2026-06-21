using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;
using AigioL.Common.Models;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Buffers.Binary;
using System.Net;
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

        var bindPcUserId = await userMembershipRepo.GetBindPCUserIdAsync(userId, cancellationToken);

        var membershipOrder = new MembershipBusinessOrder
        {
#pragma warning disable CS0618 // 类型或成员已过时
            RechargeDays = goods.RechargeDays,
#pragma warning restore CS0618 // 类型或成员已过时
            RechargeTimeSpan = goods.RechargeTimeSpan,
            PayAsYoGo = goods.PayAsYoGo,
            AmountReceivable = amountReceivable,
            UserId = userId,
            Note = "购买会员",
            GoodsNo = goods.GoodsNo,
            GoodsName = goods.GoodsName,
            MemberLicenseType = goods.MemberLicenseType,
            MembershipGoodsId = goods.Id,
            BusinessSource = MembershipBusinessSource.普通订单,
            ChannelPackageId = channelPackageId,
            BindPCUserId = bindPcUserId,
        };

        var result = await membershipBusinessOrderRepo.CreateBusinessOrder(membershipOrder, orderId: orderId);
        return result.Success ? result.Order?.Id : null;
    }

    public async Task<(bool isOK, UserMembershipChangeRecord? record)> CreateMembershipOrderByCDKeyAsync(
        Guid userId,
        MembershipProductKeyRecord productKeyRecord,
        MembershipGoods goods,
        Guid? channelPackageId = null)
    {
        if (productKeyRecord.MembershipGoodsId != goods.Id)
        {
            return (false, null);
        }

        var membershipOrder = new MembershipBusinessOrder
        {
#pragma warning disable CS0618 // 类型或成员已过时
            RechargeDays = productKeyRecord.RechargeDays,
#pragma warning restore CS0618 // 类型或成员已过时
            RechargeTimeSpan = productKeyRecord.RechargeTimeSpan,
            PayAsYoGo = productKeyRecord.PayAsYoGo,
            UserId = userId,
            Note = "CDKey 兑换会员",
            GoodsNo = goods.GoodsNo,
            GoodsName = goods.GoodsName,
            MemberLicenseType = goods.MemberLicenseType,
            MembershipGoodsId = goods.Id,
            BusinessSource = MembershipBusinessSource.CDK激活,
            ProductKeyRecordId = productKeyRecord.Id,
            ChannelPackageId = channelPackageId,
        };

        var result = await membershipBusinessOrderRepo.CreateBusinessOrder(membershipOrder);
        return (result.Success, result.record);
    }

    /// <summary>
    /// 使用 CDKey 兑换会员的 IP 锁定时间，单位分钟，超过次数限制后锁定该 IP 一段时间，减少暴力破解 CDKey 的风险
    /// </summary>
    static readonly TimeSpan CreateByCDKeyIpLockoutTimeSpan = TimeSpan.FromMinutes(10);

    public async Task<ApiRsp<TContent?>> CreateByCDKeyAsync<TContent>(
        Func<bool, UserMembershipChangeRecord, ApiRsp<TContent?>> getResult,
        string ip,
        ILogger logger,
        IConnectionMultiplexer conn,
        IDistributedCache cache,
        IMembershipProductKeyRecordRepository membershipProductKeyRecordRepo,
        IMembershipGoodsRepository membershipGoodsRepo,
        Guid userId,
        Guid cdKey,
        Guid? channelPackageId,
        bool isTimeSpan = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return "未知的 IP 地址";
        }

        //Guid cdKey;
        //var cdKeyB58 = Base58Guid.Decode(cdKeyRequest.CDKey);
        //if (cdKeyB58.HasValue)
        //{
        //    cdKey = cdKeyB58.Value;
        //}
        //else if (!ShortGuid.TryParse(cdKeyRequest.CDKey, out cdKey))
        //{
        //    return "CDKey 不合法";
        //}

        // IP Redis 键检查周期内失败次数过多限制
        var ipCacheKey = $"CreateByCDKey_Ip_[{ip}]";
        var ipAccessFailedCountB = await cache.GetAsync(ipCacheKey, cancellationToken);
        var ipAccessFailedCount = ipAccessFailedCountB == null ? 0 : BinaryPrimitives.ReadInt32BigEndian(ipAccessFailedCountB);
        if (ipAccessFailedCount >= CacheKeys.MaxIpAccessFailedCount)
        {
            return HttpStatusCode.TooManyRequests;
        }

        var lockKey = CacheKeys.GetUserRechargeOperationLockKey(cdKey);
        var r = await conn.LockHandleAsync(lockKey, HandleCoreAsync, errorHandle: ErrorHandleAsync);
        if (r.IsSuccess())
        {
            await RefreshUserMembershipCacheAsync(userId); // 兑换 CDKey 成功后刷新用户会员信息缓存
        }
        else
        {
            var ipAccessFailedCountS = new byte[sizeof(int)];
            BinaryPrimitives.WriteInt32BigEndian(ipAccessFailedCountS, ipAccessFailedCount + 1);
            await cache.SetAsync(ipCacheKey, ipAccessFailedCountS, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CreateByCDKeyIpLockoutTimeSpan,
            }, CancellationToken.None);
        }
        return r;

        async Task<ApiRsp<TContent?>> HandleCoreAsync()
        {
            var productKey = await membershipProductKeyRecordRepo.FindAsync(cdKey, cancellationToken: cancellationToken);
            if (productKey == null || productKey.IsUsed)
            {
                return "CDKey 不存在或已被激活";
            }

            var goods = await membershipGoodsRepo.FindAsync(productKey.MembershipGoodsId, cancellationToken: cancellationToken);
            if (goods == null ||
                (!goods.MemberLicenseType.HasFlag(MembershipLicenseFlags.CDKey) &&
                !goods.MemberLicenseType.HasFlag(MembershipLicenseFlags.Points)))
            {
                return "充值商品类型未找到或充值类型不匹配";
            }

            var r = await CreateMembershipOrderByCDKeyAsync(userId, productKey, goods, channelPackageId);
            if (r.isOK)
            {
                ArgumentNullException.ThrowIfNull(r.record);
                var r2 = getResult(isTimeSpan, r.record);
                return r2;
            }

            logger.LogTrace("{cdKey}({cdKeyS}) create businessOrder by cdkey failed", cdKey, cdKey);
            return "CDKey 已被使用";
        }

        Task<ApiRsp<TContent?>> ErrorHandleAsync(Exception ex)
        {
            logger.LogError(ex, "{cdKey}({cdKeyS}) create businessOrder by cdkey error", cdKey, cdKey);
            ApiRsp<TContent?> r = ApiRspCode.InternalServerError;
            return Task.FromResult(r);
        }
    }

    #region SubscribeHandle / 支付订单通知处理

    public async Task<bool> OrderPaymentSuccessHandleAsync(string orderId)
    {
        (var isSuccess, var userId) = await membershipBusinessOrderRepo.OrderPaymentSuccess(orderId);

        if (isSuccess) await RefreshUserMembershipCacheAsync(userId!.Value); // 支付订单成功通知后刷新用户会员信息缓存

        return isSuccess;
    }

    public async Task<bool> OrderPaymentRefundedHandleAsync(string orderId)
    {
        (var isSuccess, var userId) = await membershipBusinessOrderRepo.OrderRefunded(orderId);

        if (isSuccess) await RefreshUserMembershipCacheAsync(userId!.Value); // 订单退款完成通知后刷新用户会员信息缓存

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

        var rechargeDays = GetRechargeDays(agreement.PeriodType, agreement.Period);
        var membershipOrder = new MembershipBusinessOrder
        {
#pragma warning disable CS0618 // 类型或成员已过时
            RechargeDays = rechargeDays,
#pragma warning restore CS0618 // 类型或成员已过时
            RechargeTimeSpan = TimeSpan.FromDays(rechargeDays),
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

        // 创建会员订单：通用订单、业务订单、支付组成一并创建
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

        if (handleSuccess) await RefreshUserMembershipCacheAsync(business_order.UserId); // 签约商户扣款成功时处理后刷新用户会员信息缓存

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

        if (handleSuccess) await RefreshUserMembershipCacheAsync(business_order.UserId); // 签约商户解约成功时处理后刷新用户会员信息缓存

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
            isOK = await RefreshUserMembershipCacheAsync(userId); // 后台编辑用户会员时刷新用户会员信息缓存
            return isOK;
        }
        return false;
    }
}
