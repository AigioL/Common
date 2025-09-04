using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IOfficialMessageRepository : IRepository<OfficialMessage, Guid>, IEFRepository
{
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