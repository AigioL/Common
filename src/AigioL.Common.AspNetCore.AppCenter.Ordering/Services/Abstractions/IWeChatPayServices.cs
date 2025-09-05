namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;

/// <summary>
/// 微信支付服务
/// </summary>
public interface IWeChatPayServices : IUnSignAgreementServices
{
    const string V2 = "v2";

    ///// <summary>
    ///// 发起支付
    ///// </summary>
    ///// <param name="tradeType">交易类型</param>
    ///// <param name="orderNumber">商户订单号</param>
    ///// <param name="title">显示标题</param>
    ///// <param name="amount">金额</param>
    ///// <param name="body">描述</param>
    ///// <param name="ip">客户端 IP</param>
    ///// <param name="timeExpire">过期时间</param>
    ///// <param name="userOpenId">用户OpenId</param>
    ///// <param name="returnUrl">返回地址</param>
    ///// <returns></returns>
    //Task<PubPayState> PubPay(WeChatPayTradeType tradeType, string orderNumber, string title,
    //    decimal amount, string body, string ip, DateTimeOffset timeExpire, string? userOpenId = null, string? returnUrl = null);

    /// <summary>
    /// 申请退款
    /// </summary>
    /// <param name="orderNumber">商户订单号</param>
    /// <param name="refundNumber">商户退款单号</param>
    /// <param name="refundAmount">退款金额</param>
    /// <param name="totalAmount">支付金额</param>
    /// <returns></returns>
    Task<(bool Success, bool RefundSuccess, string Code, string ErrorDesc)> Refund(string orderNumber, string refundNumber, decimal refundAmount, decimal totalAmount);

    /// <summary>
    /// 关闭订单
    /// </summary>
    /// <param name="orderNumber">商户订单号</param>
    /// <returns></returns>
    Task<bool?> OrderClose(string orderNumber);

    ///// <summary>
    ///// 查询订单
    ///// </summary>
    ///// <param name="orderNumber">商户订单号</param>
    ///// <returns></returns>
    //Task<WechatPayTradeResult?> OrderQuery(string orderNumber);

    ///// <summary>
    ///// 查询退款
    ///// </summary>
    ///// <param name="orderNumber">商户订单号</param>
    ///// <param name="refundNumber">商户退款单号</param>
    ///// <returns></returns>
    //Task<RefundResult?> RefundQuery(string orderNumber, string refundNumber);

    #region 商家扣款

    ///// <summary>
    ///// 获取支付中签约链接
    ///// </summary>
    ///// <param name="configuration"></param>
    ///// <param name="tradeType"></param>
    ///// <param name="agreementNo"></param>
    ///// <param name="orderNumber"></param>
    ///// <param name="title"></param>
    ///// <param name="firstAmount"></param>
    ///// <param name="ip"></param>
    ///// <param name="timeExpire"></param>
    ///// <param name="displayAccount"></param>
    ///// <param name="userOpenId"></param>
    ///// <param name="returnUrl"></param>
    ///// <returns></returns>
    //Task<UserAgreement> GetContractOrderPageUrl(
    //    MerchantDeductionAgreementConfiguration configuration,
    //    WeChatPayTradeType tradeType,
    //    string agreementNo,
    //    string orderNumber,
    //    string title,
    //    decimal firstAmount,
    //    string ip,
    //    DateTimeOffset timeExpire,
    //    string displayAccount,
    //    string? userOpenId = null,
    //    string? returnUrl = null);

    /// <summary>
    /// 解约委托代扣协议
    /// </summary>
    /// <param name="contractId">微信支付委托代扣协议 Id</param>
    /// <returns></returns>
    new Task<bool> UnSignAgreement(string contractId);

    /// <summary>
    /// 按照商家扣款协议执行扣款
    /// </summary>
    /// <param name="orderNumber">商户订单号</param>
    /// <param name="title">订单标题</param>
    /// <param name="amount">订单金额</param>
    /// <param name="agreementNo">协议号</param>
    /// <returns></returns>
    Task<bool> ExecuteAgreementDeduction(string orderNumber, string title, decimal amount, string agreementNo);

    #endregion 商家扣款
}