using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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

partial class OfficialMessageRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<OfficialMessageTableItemModel>> QueryAsync(
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
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.OfficialMessages
            .AsNoTrackingWithIdentityResolution()
            .ProjectTo<OfficialMessageTableItemModel>(mapper.ConfigurationProvider);

        if (messageType.HasValue)
            query = query.Where(x => x.MessageType == messageType);
        if (!string.IsNullOrEmpty(title))
            query = query.Where(x => x.Title.Contains(title));
        if (pushClientDevice.HasValue)
            query = query.Where(x => x.PushClientDevice == pushClientDevice);
        if (userViewable.HasValue)
            query = query.Where(x => x.UserViewable == userViewable);
        if (pushTime != null && pushTime.Length == 2)
        {
            if (pushTime[0].HasValue)
                query = query.Where(x => x.PushTime >= pushTime[0]);
            if (pushTime[1].HasValue)
                query = query.Where(x => x.PushTime < pushTime[1]);
        }
        if (expireTime != null && expireTime.Length == 2)
        {
            if (expireTime[0].HasValue)
                query = query.Where(x => x.ExpireTime >= expireTime[0]);
            if (expireTime[1].HasValue)
                query = query.Where(x => x.ExpireTime < expireTime[1]);
        }
        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }
        var r = await query.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<AddOrEditOfficialMessageModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.OfficialMessages
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id)
            .ProjectTo<AddOrEditOfficialMessageModel>(mapper.ConfigurationProvider);
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditOfficialMessageModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = await FindAsync(model.Id, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        mapper.Map(model, entity);
        entity.OperatorUserId = operatorUserId;

        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> InsertAsync(
        Guid? createUserId,
        AddOrEditOfficialMessageModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = mapper.Map<OfficialMessage>(model);
        entity.Id = default;
        entity.CreateUserId = createUserId;

        await db.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
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