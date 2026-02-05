using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
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
    Task<ApiRsp<(Order? order, AftersalesBillDetailModel? aftersalesBillDetailModel)>> CreateAftersalesBill(
        string orderId,
        string refundReason,
        Guid? userId,
        CancellationToken cancellationToken = default);
}

partial interface IAftersalesBillRepository
{
    Task<PagedModel<AftersalesBillTableItem>> QueryAsync(
        string? orderNumber,
        string? aftersalesNumber,
        int? businessType,
        Guid? userId,
        decimal? refundAmount,
        AuditStatus[]? auditStatus,
        string? refundReason,
        string? sellerNote,
        DateTimeOffset?[]? createTime,
        DateTimeOffset?[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> AuditAsync(
        Guid? operatorUserId,
        Guid id,
        EditAftersalesBillAuditModel model);
}