using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;

public partial interface IOrderRepository
{
    /// <summary>
    /// 获取订单信息
    /// </summary>
    /// <param name="orderId">订单 Id</param>
    /// <param name="userId">用户 Id，限制用户只能操作自己的订单</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderDetailModel?> GetOrderInfo(string orderId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取支付相关的订单信息
    /// </summary>
    /// <param name="orderId">订单 Id</param>
    /// <param name="isWaitPay">是否获取等待支付状态</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<OrderPayInfoModel?> GetOrderPaymentInfo(string orderId, bool isWaitPay = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的订单列表
    /// </summary>
    Task<PagedModel<OrderItemInfoModel>> QueryUserOrderListAsync(
        Guid userId,
        long? orderNumber,
        OrderStatus[]? status,
        DateTimeOffset?[]? paymentTime,
        int? businessType,
        string? note,
        DateTimeOffset?[]? creationTime,
        int current,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过支付记录查询用户绑定外部平台信息
    /// </summary>
    /// <param name="orderNumber">商家订单号</param>
    /// <param name="paymentNumber">支付平台订单号</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(ExternalLoginChannel Channel, string? NickName)[]> GetExternalAccountInfoAsync(
        string orderNumber,
        string paymentNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询用户订单数量
    /// </summary>
    /// <param name="userId">用户 Id</param>
    /// <param name="status">订单状态</param>
    /// <param name="businessType">订单业务类型</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> GetUserOrderCountAsync(Guid userId,
        OrderStatus[]? status,
        int? businessType,
        CancellationToken cancellationToken = default);
}
