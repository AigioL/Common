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

sealed partial class UserMembershipRepository<TDbContext> :
    IUserMembershipRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserMembershipRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<bool> AddUserMembershipFlagAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        MembershipLicenseFlags membershipLicenseFlags)
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

    public async Task<bool> RemoveUserMembershipFlagAndCheckExpiredAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        MembershipLicenseFlags membershipLicenseFlags)
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

    public Task<MembershipInfo?> GetUserMembershipAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        CancellationToken cancellationToken = default)
    {
        var query = (from x in Entity.AsNoTrackingWithIdentityResolution()
                     where x.Id == userId
                     select new MembershipInfo
                     {
                         MemberLicenseFlags = x.MemberLicenseFlags,
                         StartDate = x.StartDate,
                         ExpireDate = x.ExpireDate,
                         FirstMembershipDate = x.FirstMembershipDate,
                         PayAsYoGo = x.PayAsYoGo,
                     });
        var r = query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<(MembershipInfo? membershipInfo, bool? lockTake)> GetUserMembershipCachePriorityAsync(
        ILogger? logger,
        IConnectionMultiplexer conn,
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        bool isLockTake = false,
        CancellationToken cancellationToken = default,
        bool? ignoreCache = false)
    {
        bool? lockTake = null;
        (MembershipInfo? membershipInfo, bool? lockTake) Result(MembershipInfo? membershipInfo)
        {
            return (membershipInfo, lockTake);
        }
        MembershipInfo? r = null;
        var database = conn.GetDatabase(CacheKeys.RedisMessagingDb);
        var cacheKey = CacheKeys.GetUserMembershipCacheKey(userId);
        // 如果忽略缓存，则直接从数据库获取数据，并更新缓存
        if (ignoreCache.HasValue && ignoreCache.Value)
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
                var defaultExpireTime = UserMembershipRepositoryHelper.GetMembershipCacheTtl(r, DateTimeOffset.Now);
                await database.StringSetAsync(cacheKey, serializeData, defaultExpireTime);
                return Result(r);
            }
        }
        ReadOnlyMemory<byte> data = await database.StringGetAsync(cacheKey);
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
                        var defaultExpireTime = UserMembershipRepositoryHelper.GetMembershipCacheTtl(r, DateTimeOffset.Now);
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

    public async Task<int?> DeductionPayAsYoGoAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        TimeSpan changeValue,
        DateTimeOffset? now = null)
    {
        now ??= DateTimeOffset.UtcNow;
        var query = db.UserMemberships.Where(x => x.Id == userId);

        // 过期保护：包月未过期时禁止扣减按量付费时长
        var expireDate = await query.Select(x => x.ExpireDate).SingleOrDefaultAsync();
        if (expireDate != default && expireDate >= now.Value)
        {
            return null;
        }

        var rowCount = await query.ExecuteUpdateAsync(p => p
            .SetProperty(x => x.UpdateTime, now.Value)
            .SetProperty(x => x.PayAsYoGo, y => (y.PayAsYoGo - changeValue > TimeSpan.Zero ? y.PayAsYoGo - changeValue : TimeSpan.Zero)));

        if (rowCount > 0)
        {
            var (direction, payAsYoGo) = UserMembershipRepositoryHelper.CreatePayAsYoGoDeductionChange(changeValue);
            UserMembershipChangeRecord record = new()
            {
                UserId = userId,
                MembershipChangeDirection = direction,
                PayAsYoGo = payAsYoGo,
                Note = "按量付费的扣费",
                CurrentRealExpireDate = expireDate,
                CreateTime = now.Value,
            };
            await db.UserMembershipChangeRecords.AddAsync(record);
            rowCount += await db.SaveChangesAsync();
        }

        return rowCount;
    }

    public async Task<int> EditUserMembershipAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
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

    public async Task<Guid?> GetBindPCUserIdAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        CancellationToken cancellationToken = default)
    {
        var query = db.UserMemberships
            .Where(x => x.Id == userId && x.BindPCUserExpireDate >= DateTimeOffset.UtcNow)
            .Select(x => x.BindPCUserId);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }
}

#if !USE_NUM_UID
partial class UserMembershipRepository<TDbContext> : Repository<TDbContext, UserMembership, Guid>;
#else
partial class UserMembershipRepository<TDbContext> : Repository<TDbContext, UserMembership, long>;
#endif

internal static class UserMembershipRepositoryHelper
{
    internal static TimeSpan GetMembershipCacheTtl(
        MembershipInfo membershipInfo,
        DateTimeOffset now,
        TimeSpan? defaultExpireTime = null)
    {
        var cacheTtl = defaultExpireTime ?? TimeSpan.FromMinutes(5);
        if (membershipInfo.ExpireDate.HasValue && membershipInfo.ExpireDate.Value > now)
        {
            var expire = membershipInfo.ExpireDate.Value - now;
            if (expire < cacheTtl)
            {
                cacheTtl = expire;
            }
        }
        return cacheTtl;
    }

    internal static (MembershipChangeDirection direction, TimeSpan payAsYoGo) CreatePayAsYoGoDeductionChange(TimeSpan changeValue)
    {
        return changeValue > TimeSpan.Zero
            ? (MembershipChangeDirection.Out, changeValue.Negate())
            : (MembershipChangeDirection.In, changeValue.Negate());
    }
}