using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

sealed partial class OfficialMessageRepository<TDbContext> :
    Repository<TDbContext, OfficialMessage, Guid>,
    IOfficialMessageRepository
    where TDbContext : DbContext, IOfficialMessageDbContext
{
    public OfficialMessageRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class OfficialMessageRepository<TDbContext> // 客户端
{
    public async Task<PagedModel<OfficialMessageItemModel>> QueryAsync(
        IReadOnlyAppVer? appVer,
        ClientPlatform? clientPlatform,
        OfficialMessageType? messageType,
        int current,
        int pageSize)
    {
#pragma warning disable CS0618 // 类型或成员已过时
        var clientAppVersionId = appVer?.Id;
#pragma warning restore CS0618 // 类型或成员已过时

        // 查询出 推送时间已到 且 未过期时间未到 的官方消息
        IQueryable<OfficialMessage> query = EntityNoTracking
            .Where(x => x.PushTime <= DateTime.UtcNow)
            .Where(x => !x.ExpireTime.HasValue || DateTime.UtcNow < x.ExpireTime)
            .OrderByDescending(x => x.PushTime)
            .ThenBy(x => x.ExpireTime)
            .ThenBy(x => x.Id);

        if (clientAppVersionId.HasValue && clientAppVersionId.Value != default)
        {
            query = query.Where(x => !(x.TargetAppVerRelations!.Count > 0) ||
                x.TargetAppVerRelations!.Any(y => y.AppVerId == clientAppVersionId.Value));
        }

        if (messageType.HasValue && messageType != OfficialMessageType.News) // 最新消息不需要区分类型
            query = query.Where(x => x.MessageType == messageType);
        if (clientPlatform.HasValue)
            query = query.Where(x => x.PushClientDevice.HasFlag(clientPlatform.Value));

        var query2 = query.Select(FExpressions.MapToItemDTO);

        var r = await query2.PagingAsync(current, pageSize, RequestAborted);
        return r;
    }
}

file static class FExpressions
{
    internal static readonly Expression<Func<OfficialMessage, OfficialMessageItemModel>> MapToItemDTO =
        x => new OfficialMessageItemModel
        {
            Title = x.Title ?? "",
            Content = x.Content ?? "",
            MessageLink = x.MessageLink,
            PushTime = x.PushTime,
        };
}