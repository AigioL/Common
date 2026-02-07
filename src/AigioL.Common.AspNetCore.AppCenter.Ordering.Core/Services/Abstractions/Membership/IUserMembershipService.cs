using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.Models;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Order = AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Order;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;

public interface IUserMembershipService
{
    /// <summary>
    /// 创建会员业务订单
    /// </summary>
    Task<string?> CreateMembershipOrderAsync(
        Guid userId,
        MembershipGoods goods,
        Guid? channelPackageId,
        string? orderId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用 CDKey 兑换会员
    /// </summary>
    Task<ApiRsp<TContent?>> CreateByCDKeyAsync<TContent>(
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
        CancellationToken cancellationToken = default);

    #region SubscribeHandle / 支付订单通知处理

    /// <summary>
    /// 支付成功处理
    /// </summary>
    Task<bool> OrderPaymentSuccessHandleAsync(string orderId);

    /// <summary>
    /// 退款处理
    /// </summary>
    Task<bool> OrderPaymentRefundedHandleAsync(string orderId);

    /// <summary>
    /// 取消订单处理
    /// </summary>
    Task<bool> OrderPaymentCancelHandleAsync(string orderId);

    #endregion

    #region MerchantDeduction / 商户扣款（连续订阅处理）

    /// <summary>
    /// 商户签约扣款时创建业务订单
    /// </summary>
    Task<Order?> CreateMembershipOrderByMerchantDeductionAsync(MerchantDeductionAgreement agreement);

    /// <summary>
    /// 签约商户扣款成功时处理
    /// </summary>
    Task<bool> SignMerchantDeductionSuccessHandleAsync(string agreementNo);

    /// <summary>
    /// 解约商户扣款成功时处理
    /// </summary>
    Task<bool> UnSignMerchantDeductionSuccessHandleAsync(string agreementNo);

    #endregion

    /// <summary>
    /// 刷新用户会员信息缓存
    /// </summary>
    Task<bool> RefreshUserMembershipCacheAsync(Guid userId);

    /// <summary>
    /// 编辑用户会员时长并刷新用户会员信息缓存
    /// </summary>
    Task<bool> EditUserMembershipWithRefreshUserMembershipCacheAsync(
        Guid userId,
        Guid? bmUserId,
        DateTimeOffset? endTime,
        TimeSpan? timeSpan,
        string? note);
}
