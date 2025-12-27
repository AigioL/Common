using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IUserWalletChangeRecordRepository
{
}

partial interface IUserWalletChangeRecordRepository // 管理后台
{
    /// <summary>
    /// 后台表格查询
    /// </summary>
    /// <param name="userId">用户 Id</param>
    /// <param name="event">事件</param>
    /// <param name="type">值类型</param>
    /// <param name="direction">支付方向</param>
    /// <param name="note">备注</param>
    /// <param name="sourceId">来源 Id</param>
    /// <param name="noticeStatus">通知状态</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="current">当前页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="cancellationToken"></param>
    /// <returns>分页模型</returns>
    Task<PagedModel<UserWalletChangeRecordModel>> QueryAsync(
        Guid? userId,
        UserWalletValueEvent[]? @event,
        UserWalletValueType[]? type,
        UserWalletPaymentDirection? direction,
        string? note,
        string? sourceId,
        bool? noticeStatus,
        DateTimeOffset?[]? createTime,
        int current,
        int pageSize,
        CancellationToken cancellationToken = default);
}