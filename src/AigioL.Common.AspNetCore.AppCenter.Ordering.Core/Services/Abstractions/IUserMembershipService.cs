using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;

public interface IUserMembershipService
{
    /// <summary>
    /// 创建会员业务订单
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="goods"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<Guid?> CreateMembershipOrderAsync(
        Guid userId,
        MembershipGoods goods);

    /// <summary>
    /// CDKey 兑换瓦特会员
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productKeyRecord"></param>
    /// <param name="goods"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> CreateMembershipOrderByCDKeyAsync(
        Guid userId,
        MembershipProductKeyRecord productKeyRecord,
        MembershipGoods goods);

    #region SubscribeHandle / 支付订单通知处理

    /// <summary>
    /// 支付成功处理
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> OrderPaymentSuccessHandleAsync(Guid orderId);

    /// <summary>
    /// 退款处理
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> OrderPaymentRefundedHandleAsync(Guid orderId);

    /// <summary>
    /// 取消订单处理
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> OrderPaymentCancelHandleAsync(Guid orderId);

    #endregion

    #region MerchantDeduction / 商户扣款（连续订阅处理）

    /// <summary>
    /// 商户签约扣款时创建业务订单
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<Order?> CreateMembershipOrderByMerchantDeductionAsync(MerchantDeductionAgreement agreement);

    /// <summary>
    /// 签约商户扣款成功时处理
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> SignMerchantDeductionSuccessHandleAsync(string agreementNo);

    /// <summary>
    /// 解约商户扣款成功时处理
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<bool> UnSignMerchantDeductionSuccessHandleAsync(string agreementNo);

    #endregion
}
