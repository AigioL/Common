using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;

public interface IOrderRepository
{
    /// <summary>
    /// 获取订单信息
    /// </summary>
    /// <param name="orderId">订单 Id</param>
    /// <param name="userId">用户 Id，限制用户只能操作自己的订单</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderDetailModel?> GetOrderInfo(Guid orderId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取支付相关的订单信息
    /// </summary>
    /// <param name="orderId">订单 Id</param>
    /// <param name="isWaitPay">是否获取等待支付状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderPayInfoModel?> GetOrderPaymentInfo(Guid orderId, bool isWaitPay = false, CancellationToken cancellationToken = default);
}
