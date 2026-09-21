using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

public sealed partial class UserMembershipChangeRecordRepository<TDbContext> :
    Repository<TDbContext, UserMembershipChangeRecord, Guid>,
    IUserMembershipChangeRecordRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserMembershipChangeRecordRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class UserMembershipChangeRecordRepository<TDbContext>
{
    public async Task<PagedModel<UserMembershipChangeRecordModel>> QueryAsync(
#if !USE_NUM_UID
        Guid? userId,
#else
        long? userId,
#endif
        MembershipChangeDirection? membershipChangeDirection,
        MembershipLicenseFlags? memberLicenseType,
        bool? isPayAsYoGo,
        string? note,
        DateTimeOffset?[]? createTime,
        int current,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<UserMembershipChangeRecord> query = db.UserMembershipChangeRecords
           .AsNoTrackingWithIdentityResolution()
           .OrderByDescending(a => a.CreateTime)
           .ThenBy(a => a.Id);

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId);
        if (membershipChangeDirection.HasValue)
        {
            var membershipChangeDirectionValue = membershipChangeDirection.Value;
            query = query.Where(x => x.MembershipChangeDirection == membershipChangeDirectionValue);
        }
        if (memberLicenseType.HasValue)
        {
            var memberLicenseTypeValue = memberLicenseType.Value;
            query = query.Where(x => x.MemberLicenseType.HasFlag(memberLicenseTypeValue));
        }
        if (isPayAsYoGo.HasValue)
        {
            var isPayAsYoGoValue = isPayAsYoGo.Value;
            query = query.Where(x => x.IsPayAsYoGo == isPayAsYoGoValue);
        }
        if (!string.IsNullOrEmpty(note))
            query = query.Where(x => x.Note!.Contains(note));
        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }

        var r = await query
            .ProjectTo<UserMembershipChangeRecordModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }
}
