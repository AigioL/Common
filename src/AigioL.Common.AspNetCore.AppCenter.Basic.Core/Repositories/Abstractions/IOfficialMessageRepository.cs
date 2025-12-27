using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IOfficialMessageRepository : IRepository<OfficialMessage, Guid>, IEFRepository
{
}

partial interface IOfficialMessageRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="title">标题</param>
    /// <param name="pushClientDevice">推送客户端设备</param>
    /// <param name="pushTime">推送时间范围</param>
    /// <param name="expireTime">过期时间范围</param>
    /// <param name="createTime">创建时间范围</param>
    /// <param name="userViewable">用户可见</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">是否降序</param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedModel<OfficialMessageTableItemModel>> QueryAsync(
        OfficialMessageType? messageType,
        string? title,
        ClientPlatform? pushClientDevice,
        DateTimeOffset?[]? pushTime,
        DateTimeOffset?[]? expireTime,
        DateTimeOffset?[]? createTime,
        bool? userViewable,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<AddOrEditOfficialMessageModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditOfficialMessageModel model,
        CancellationToken cancellationToken = default);

    Task<bool> InsertAsync(
        Guid? createUserId,
        AddOrEditOfficialMessageModel model,
        CancellationToken cancellationToken = default);
}

partial interface IOfficialMessageRepository // 客户端
{
    /// <summary>
    /// 查询官方消息
    /// </summary>
    /// <param name="appVer">客户端版本</param>
    /// <param name="clientPlatform">客户端平台</param>
    /// <param name="messageType">消息类型</param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<PagedModel<OfficialMessageItemModel>> QueryAsync(
        IReadOnlyAppVer? appVer,
        ClientPlatform? clientPlatform,
        OfficialMessageType? messageType,
        int current,
        int pageSize);
}