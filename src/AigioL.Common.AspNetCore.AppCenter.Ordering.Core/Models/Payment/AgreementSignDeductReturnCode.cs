namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 商家扣款协议签约返回代码
/// </summary>
public static partial class AgreementSignDeductReturnCode
{
    public static readonly (string Code, string Message) Success = ("000", "");

    public static readonly (string Code, string Message) ApiException = ("050", "接口异常");

    public static readonly (string Code, string Message) UserNotExist = ("100", "用户不存在");
    public static readonly (string Code, string Message) PaymentTypeNotSupported = ("101", "不支持的支付类型");
    public static readonly (string Code, string Message) RefundInfoNoFound = ("102", "找不到退款信息");
    public static readonly (string Code, string Message) MerchantDeductionAgreementNotExist = ("103", "商家扣款协议不存在");

    public static readonly (string Code, string Message) OrderParameterError = ("104", "订单参数错误");
    public static readonly (string Code, string Message) OrderMissingOutTradeNoParameter = ("105", "订单缺少请求方订单号参数");
    public static readonly (string Code, string Message) OrderAmountReceivableMustBeGreaterThanZero = ("106", "订单应收金额需要大于 0");
    public static readonly (string Code, string Message) OrderUserIdCannotBeEmpty = ("107", "订单用户不能为空");
    public static readonly (string Code, string Message) OrderPaymentTypeParameterError = ("108", "订单支付类型错误");
    public static readonly (string Code, string Message) SameOutTradeNoAlreadyExists = ("109", "相同的请求方订单号已存在");

    public static readonly (string Code, string Message) CreatePaymentSystemOrderFail = ("110", "创建支付订单失败");
    public static readonly (string Code, string Message) OrderNotFound = ("111", "找不到该订单");

    public static readonly (string Code, string Message) OrderIsNotPaidOrCompletedAndCannotBeRefunded = ("112", "订单不是已付款或已完成状态，无法进行退款");
    public static readonly (string Code, string Message) OrderHasBeenRefundedAndRefundCannotBeContinued = ("113", "订单已退款，无法继续退款");
    public static readonly (string Code, string Message) CurrentStatusOfOrderCannotBeRefunded = ("114", "订单当前状态无法进行退款");
    public static readonly (string Code, string Message) RefundAmountCannotBeGreaterThanOrderAmount = ("115", "退款金额退款不能大于订单金额");
    public static readonly (string Code, string Message) RefundAmountMustBeGreaterThan0 = ("116", "退款金额退款必须大于0");
    public static readonly (string Code, string Message) OrderIsBeingRefunded = ("117", "订单正在进行退款中");

    public static readonly (string Code, string Message) OrderStatusCannotBeClosed = ("118", "订单当前状态无法进行关闭");

    public static readonly (string Code, string Message) AgreementNotFound = ("119", "未找到商家扣款协议");
    public static readonly (string Code, string Message) AgreementCurrentStatusCannotUnSign = ("120", "当前状态无法解除商家扣款协议");

    public static readonly (string Code, string Message) WeChatPayMerchantDeductionNotSupported = ("121", "暂不支付微信支付的商家扣款");
    public static readonly (string Code, string Message) AgreementUnSignFail = ("122", "商家扣款协议解除失败");

    public static readonly (string Code, string Message) CloseOrderFail = ("123", "关闭订单失败，交易状态异常");
    public static readonly (string Code, string Message) OrderClosed = ("124", "订单已关闭，无法重复关闭");

    public static readonly (string Code, string Message) OrderHaveRefunded = ("125", "订单已完成退款");
    public static readonly (string Code, string Message) OrderRefundError = ("126", "订单退款出错，请联管理员处理");
    public static readonly (string Code, string Message) SameOutRefundNoAlreadyExists = ("127", "相同的请求方退款单号已存在");

    public static readonly (string Code, string Message) OrderHaveClosed = ("128", "订单已关闭");
    public static readonly (string Code, string Message) AgreementConfigurationNotFound = ("129", "未找到协议配置");

    public static readonly (string Code, string Message) AgreementNoAlreadyExists = ("130", "相同的商家扣款协议号已存在");
    public static readonly (string Code, string Message) AgreementAlreadySigned = ("131", "本扣款协议已签约");
    public static readonly (string Code, string Message) UserSignedAgreement = ("132", "已签约扣款协议");
    public static readonly (string Code, string Message) OrderHavePaid = ("133", "订单已支付");
    public static readonly (string Code, string Message) OrderIsWaitingPayInOtherPlatform = ("134", "当前订单已使用其它支付方式进行，请刷新二维码后重试");
    public static readonly (string Code, string Message) PaymentTimeout = ("135", "支付操作超时");

    public static readonly (string Code, string Message) PaymentSystemStopped = ("500", "支付系统维护中");
    public static readonly (string Code, string Message) ConfigurationInvalid = ("510", "配置错误");
}
