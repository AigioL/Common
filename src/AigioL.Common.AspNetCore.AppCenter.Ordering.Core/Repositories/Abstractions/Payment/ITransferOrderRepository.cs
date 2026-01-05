using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;

public interface ITransferOrderRepository
{
    /// <summary>
    /// 查询转账订单
    /// </summary>
    Task<TransferOrder?> GetById(Guid transferOrderId);

    /// <summary>
    /// 添加转账订单
    /// </summary>
    Task AddTransferOrder(TransferOrder transferOrder);

    /// <summary>
    /// 更新转账订单
    /// </summary>
    Task UpdateTransferOrderResult(TransferOrder transferOrder);
}
