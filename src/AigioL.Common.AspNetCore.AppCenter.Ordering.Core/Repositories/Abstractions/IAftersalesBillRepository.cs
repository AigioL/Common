using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;

public partial interface IAftersalesBillRepository : IRepository<AftersalesBill, Guid>, IEFRepository
{
    /// <summary>
    /// 创建售后单，当返回成功时，内容必定不为 <see langword="null"/>
    /// </summary>
    /// <param name="orderId">订单 Id</param>
    /// <param name="refundReason">退款原因</param>
    /// <param name="userId">用户 Id，限制用户只能操作自己的订单</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp<(Order? order, AftersalesBillDetailModel? aftersalesBill)>> CreateAftersalesBill(
        Guid orderId,
        string refundReason,
        Guid userId,
        CancellationToken cancellationToken = default);
}
