using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;

public partial interface IPaymentRepository
{
    /// <summary>
    /// 获取订单业务类型的支付方式设置
    /// </summary>
    /// <param name="businessType">订单业务类型</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderBusinessPaymentMethod[]> GetPaymentMethodAsync(int businessType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 完成订单支付
    /// </summary>
    /// <param name="orderPaidInfo"></param>
    Task CompletePaymentForOrder(OrderPaymentSuccessInfo orderPaidInfo);

    /// <summary>
    /// 完成订单退款
    /// </summary>
    Task CompleteRefundForOrderAsync(OrderRefundSuccessInfo refundInfo);

    /// <summary>
    /// 获取支付组成支付状态
    /// </summary>
    Task<bool> GetPaymentCompositionStateAsync(string orderId, OrderBusinessPaymentMethod method,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取支付相关的订单信息
    /// </summary>
    /// <param name="orderId">订单Id</param>
    /// <param name="isWaitPay">是否获取等待支付状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderPayInfoModel?> GetOrderPaymentInfoAsync(string orderId, bool isWaitPay = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 验证支付方式是否可用
    /// </summary>
    /// <param name="type">业务类型</param>
    /// <param name="method">支付方式</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsPaymentMethodValidAsync(int type, OrderBusinessPaymentMethod method,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 向订单添加支付方式
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="amount">支付金额</param>
    /// <param name="method">支付方式</param>
    /// <returns></returns>
    Task<OrderPaymentComposition?> AddOrGetPayMethodAsync(string orderId, decimal amount, OrderBusinessPaymentMethod method);

    /// <summary>
    /// 关闭支付
    /// </summary>
    Task ClosePayment(Guid paymentId);

    /// <summary>
    /// 检查待支付的订单
    /// </summary>
    /// <param name="orderNumber">订单号</param>
    /// <param name="paymentType">支付平台</param>
    Task<(Order Order, OrderPaymentComposition OrderPaymentComposition)?> GetOrderPayment(string orderNumber, PaymentType paymentType,
        CancellationToken cancellationToken = default);
}
