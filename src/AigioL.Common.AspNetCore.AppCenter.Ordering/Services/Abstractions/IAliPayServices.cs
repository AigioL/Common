namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;

/// <summary>
/// 支付宝服务
/// </summary>
public interface IAliPayServices : IUnSignAgreementServices
{
    ///// <summary>
    ///// 发起支付
    ///// </summary>
    ///// <param name="tradeType">支付类型</param>
    ///// <param name="orderId">订单 Id</param>
    ///// <param name="orderNumber">商户订单号</param>
    ///// <param name="title">标题</param>
    ///// <param name="amount">金额</param>
    ///// <param name="body">描述</param>
    ///// <param name="timeExpire">过期时间</param>
    ///// <param name="returnUrl">返回地址</param>
    ///// <returns></returns>
    //Task<PubPayState> PubPay(AliPayPayTradeType tradeType, Guid orderId, string orderNumber,
    //    string title, decimal amount, string body, DateTimeOffset timeExpire, string? returnUrl = null);

    /// <summary>
    /// 交易退款
    /// </summary>
    /// <param name="orderNumber">商户订单号</param>
    /// <param name="refundNumber">商户退款单号</param>
    /// <param name="amount">退款金额</param>
    /// <returns></returns>
    Task<(bool Success, bool RefundSuccess, string Code, string ErrorDesc)> Refund(string orderNumber, string refundNumber, decimal amount);

    /// <summary>
    /// 交易关闭
    /// </summary>
    /// <param name="orderNumber">商户订单号</param>
    /// <returns></returns>
    Task<bool?> TradeClose(string orderNumber);

    ///// <summary>
    ///// 单笔转账
    ///// </summary>
    ///// <param name="outBizNo">商户转账单号</param>
    ///// <param name="transAmount">转账金额</param>
    ///// <param name="title">标题</param>
    ///// <param name="userOpenId">支付宝用户OpenId</param>
    ///// <returns></returns>
    //Task<PubTransferState> Transfer(string outBizNo, decimal transAmount, string title, string userOpenId);

    ///// <summary>
    ///// 交易查询
    ///// </summary>
    ///// <param name="orderNumber">商户订单号</param>
    ///// <returns></returns>
    //Task<AliPayTradeResult?> TradeQuery(string orderNumber);

    ///// <summary>
    ///// 交易退款查询
    ///// </summary>
    ///// <param name="orderNumber">商户订单号</param>
    ///// <param name="refundNumber">商户退款单号</param>
    ///// <returns></returns>
    //Task<RefundResult?> RefundQuery(string orderNumber, string refundNumber);

    #region 商家扣款

    ///// <summary>
    ///// 获取商家扣款协议页面地址
    ///// </summary>
    ///// <param name="configuration">协议配置</param>
    ///// <param name="outAgreementNo">商家协议号</param>
    ///// <returns></returns>
    //Task<UserAgreement> GetAgreementSignPageUrl(MerchantDeductionAgreementConfiguration configuration, string outAgreementNo);

    /// <summary>
    /// 解约商家扣款协议
    /// </summary>
    /// <param name="agreementNo">支付宝协议号</param>
    /// <returns></returns>
    new Task<bool> UnSignAgreement(string agreementNo);

    /// <summary>
    /// 延期商家扣款协议的下次执行时间
    /// </summary>
    /// <param name="agreementNo">协议号</param>
    /// <param name="deductTime">下一次扣款时间</param>
    /// <param name="memo">修改原因</param>
    /// <returns></returns>
    Task<bool> DelayAgreementExecutionPlan(string agreementNo, DateTimeOffset deductTime, string memo);

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

    ///// <summary>
    ///// 获取支付并签约页面地址（小程序中JSAPI）
    ///// </summary>
    ///// <param name="configuration"></param>
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
    //Task<UserAgreement> GetPayAndAgreementSignPageUrl(MerchantDeductionAgreementConfiguration configuration, string agreementNo, string orderNumber, string title, decimal firstAmount, string ip, DateTimeOffset timeExpire, string displayAccount, string? userOpenId = null, string? returnUrl = null);

    Task<string?> GetUserOpenId(string code);

    string GetMiniProgramPayUrl(string continueUrl, string agreementNo);
}