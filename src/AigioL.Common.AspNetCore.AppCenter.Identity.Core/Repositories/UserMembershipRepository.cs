using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using MemoryPack;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class UserMembershipRepository<TDbContext>(TDbContext dbContext, IServiceProvider serviceProvider) :
    Repository<TDbContext, UserMembership, Guid>(dbContext, serviceProvider),
    IUserMembershipRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public async Task<bool> AddUserMembershipFlagAsync(Guid userId, MembershipLicenseFlags membershipLicenseFlags)
    {
        var flags = Enum.GetValues<MembershipLicenseFlags>().Where(x => membershipLicenseFlags.HasFlag(x)).ToArray();
        if (flags.Length > 2)
        {
            return false;
        }

        var query = from x in Entity.AsNoTrackingWithIdentityResolution()
                    where x.Id == userId && !x.MemberLicenseFlags.HasFlag(membershipLicenseFlags)
                    select x;

        var membershipLicenseFlagsInt32 = (int)membershipLicenseFlags;
        var count = await query.ExecuteUpdateAsync(e =>
            e.SetProperty(
                s => s.MemberLicenseFlags,
                s => s.MemberLicenseFlags + membershipLicenseFlagsInt32));
        return count > 0;
    }

    public async Task<bool> RemoveUserMembershipFlagAndCheckExpiredAsync(Guid userId, MembershipLicenseFlags membershipLicenseFlags)
    {
        var flags = Enum.GetValues<MembershipLicenseFlags>().Where(x => membershipLicenseFlags.HasFlag(x)).ToArray();
        if (flags.Length > 2)
        {
            return false;
        }

        var query = from x in Entity.AsNoTrackingWithIdentityResolution()
                    where x.Id == userId && !x.MemberLicenseFlags.HasFlag(membershipLicenseFlags)
                    select x;

        var membershipLicenseFlagsInt32 = (int)membershipLicenseFlags;
        var count = await query.ExecuteUpdateAsync(e =>
            e.SetProperty(
                s => s.MemberLicenseFlags,
                s => s.MemberLicenseFlags - membershipLicenseFlagsInt32));

        var realExpireDate = await db.UserMemberships.AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == userId)
            .Select(s => s.ExpireDate)
            .FirstOrDefaultAsync();
        if (realExpireDate != default && realExpireDate <= DateTimeOffset.Now)
        {
            var count2 = await db.Users.AsNoTrackingWithIdentityResolution()
                .Where(x => x.Id == userId && x.UserType.HasFlag(UserType.Membership))
                .ExecuteUpdateAsync(e =>
                    e.SetProperty(
                        s => s.UserType,
                        s => s.UserType - (int)UserType.Membership));
            return count2 > 0;
        }
        return count > 0;
    }

    public Task<MembershipInfo?> GetUserMembershipAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = (from x in Entity.AsNoTrackingWithIdentityResolution()
                     where x.Id == userId
                     select new MembershipInfo
                     {
                         MemberLicenseFlags = x.MemberLicenseFlags,
                         StartDate = x.StartDate,
                         ExpireDate = x.ExpireDate,
                         FirstMembershipDate = x.FirstMembershipDate,
                     });
        var r = query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<(MembershipInfo? membershipInfo, bool? lockTake)> GetUserMembershipCachePriorityAsync(
        ILogger? logger,
        IConnectionMultiplexer conn,
        Guid userId,
        bool isLockTake = false,
        CancellationToken cancellationToken = default)
    {
        bool? lockTake = null;
        (MembershipInfo? membershipInfo, bool? lockTake) Result(MembershipInfo? membershipInfo)
        {
            return (membershipInfo, lockTake);
        }

        var database = conn.GetDatabase(CacheKeys.RedisMessagingDb);

        var cacheKey = CacheKeys.GetUserMembershipCacheKey(userId);
        ReadOnlyMemory<byte> data = await database.StringGetAsync(cacheKey);

        MembershipInfo? r = null;
        if (data.Length <= 0)
        {
            IDatabase? lockDb = null;
            string? lockValue = null;
            if (isLockTake)
            {
                lockValue = Guid.NewGuid().ToString();
                lockDb = conn.GetDatabase(CacheKeys.RedisLockDb);
                lockTake = await lockDb.LockTakeAsync(cacheKey, lockValue, TimeSpan.FromMinutes(1));
                if (lockTake.HasValue && !lockTake.Value)
                {
                    return Result(null);
                }
            }
            try
            {
                // 二次检查
                data = await database.StringGetAsync(cacheKey);
                if (data.Length <= 0)
                {
                    r = await GetUserMembershipAsync(userId, cancellationToken);

                    // 用户不存在会员信息时，返回空对象
                    if (r == null)
                    {
                        return Result(new());
                    }
                    else
                    {
                        var serializeData = MemoryPackSerializer.Serialize(r);

                        var defaultExpireTime = TimeSpan.FromMinutes(5);
                        if (r.IsMembership)
                        {
                            var expire = r.ExpireDate!.Value - DateTimeOffset.Now;
                            if (expire < defaultExpireTime)
                                defaultExpireTime = expire;
                        }
                        await database.StringSetAsync(cacheKey, serializeData, defaultExpireTime);
                        return Result(r);
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    LogErrorOnGetUserMembership(logger, ex);
                }
            }
            finally
            {
                if (lockDb != null && lockValue != null)
                {
                    await lockDb.LockReleaseAsync(cacheKey, lockValue);
                }
            }
        }

        if (data.Length > 0)
        {
            r = MemoryPackSerializer.Deserialize<MembershipInfo>(data.Span);
        }

        return Result(r);
    }

    public async Task<int> EditUserMembershipAsync(
        Guid userId,
        Guid? bmUserId,
        DateTimeOffset? endTime,
        TimeSpan? timeSpan,
        string? note)
    {
        var now = DateTimeOffset.UtcNow;
        var query = db.UserMemberships.Where(x => x.Id == userId);
        int rowCount;
        TimeSpan changeValue;
        if (timeSpan.HasValue)
        {
            var expireDate = now + timeSpan.Value;
            rowCount = await query.ExecuteUpdateAsync(p => p
                .SetProperty(x => x.UpdateTime, now)
                .SetProperty(x => x.FirstMembershipDate, x => x.FirstMembershipDate == default ? now : x.FirstMembershipDate)
                .SetProperty(x => x.StartDate, x => x.StartDate == default ? now : x.FirstMembershipDate)
                .SetProperty(x => x.ExpireDate, x => x.ExpireDate == default ? expireDate : x.ExpireDate.Add(timeSpan.Value))
            );
            changeValue = timeSpan.Value;
        }
        else if (endTime.HasValue)
        {
            var expireDate = await query.Select(x => x.ExpireDate).SingleOrDefaultAsync();
            rowCount = await query.ExecuteUpdateAsync(p => p
                .SetProperty(x => x.UpdateTime, now)
                .SetProperty(x => x.FirstMembershipDate, x => x.FirstMembershipDate == default ? now : x.FirstMembershipDate)
                .SetProperty(x => x.StartDate, x => x.StartDate == default ? now : x.FirstMembershipDate)
                .SetProperty(x => x.ExpireDate, endTime.Value)
            );
            changeValue = endTime.Value - (expireDate == default ? now : expireDate);
        }
        else
        {
            return 0;
        }

        if (rowCount > 0)
        {
            var expireDate = await query.Select(x => x.ExpireDate).SingleOrDefaultAsync();
            UserMembershipChangeRecord record = new()
            {
                UserId = userId,
                MembershipChangeDirection = changeValue < TimeSpan.Zero ? MembershipChangeDirection.Out : MembershipChangeDirection.In,
                Value = changeValue,
                Note = note,
                CurrentRealExpireDate = expireDate,
                CreateTime = now,
                CreateUserId = bmUserId,
            };
            await db.UserMembershipChangeRecords.AddAsync(record);
            rowCount += await db.SaveChangesAsync();
        }

        return rowCount;
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "GetUserMembership fail")]
    private static partial void LogErrorOnGetUserMembership(
        ILogger logger, Exception ex);
}