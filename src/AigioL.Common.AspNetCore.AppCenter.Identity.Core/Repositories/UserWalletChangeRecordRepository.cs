using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

public sealed partial class UserWalletChangeRecordRepository<TDbContext> :
    Repository<TDbContext, UserWalletChangeRecord, Guid>,
    IUserWalletChangeRecordRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserWalletChangeRecordRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class UserWalletChangeRecordRepository<TDbContext>
{
    public async Task<PagedModel<UserWalletChangeRecordModel>> QueryAsync(
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
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<UserWalletChangeRecord> query = db.UserWalletChangeRecords
           .AsNoTrackingWithIdentityResolution()
           .OrderByDescending(a => a.CreateTime)
           .ThenBy(a => a.Id);

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId);
        if (@event?.Length > 0)
            query = query.Where(x => @event.Contains(x.Event));
        if (type?.Length > 0)
            query = query.Where(x => type.Contains(x.Type));
        if (direction.HasValue)
            query = query.Where(x => x.Direction == direction);
        if (!string.IsNullOrEmpty(note))
            query = query.Where(x => x.Note!.Contains(note));
        if (!string.IsNullOrEmpty(sourceId))
            query = query.Where(x => x.SourceId == sourceId);
        if (noticeStatus.HasValue)
            query = query.Where(x => x.NoticeStatus == noticeStatus);
        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }

        var r = await query
            .ProjectTo<UserWalletChangeRecordModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }
}
