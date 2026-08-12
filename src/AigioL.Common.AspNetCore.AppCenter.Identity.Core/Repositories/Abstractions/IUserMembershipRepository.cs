using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using StackExchange.Redis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IUserMembershipRepository : IEFRepository
{
    /// <summary>
    /// 增加用户会员订阅类型
    /// </summary>
    Task<bool> AddUserMembershipFlagAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        MembershipLicenseFlags membershipLicenseFlags);

    /// <summary>
    /// 去除用户指定订阅类型并检查会员是否过期
    /// </summary>
    Task<bool> RemoveUserMembershipFlagAndCheckExpiredAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        MembershipLicenseFlags membershipLicenseFlags);

    /// <summary>
    /// 获取用户会员信息
    /// </summary>
    Task<MembershipInfo?> GetUserMembershipAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户会员信息（缓存优先）
    /// </summary>
    Task<(MembershipInfo? membershipInfo, bool? lockTake)> GetUserMembershipCachePriorityAsync(
        ILogger? logger,
        IConnectionMultiplexer conn,
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        bool isLockTake = false,
        CancellationToken cancellationToken = default,
        bool? ignoreCache = false);

    /// <summary>
    /// 按量付费的扣费
    /// </summary>
    Task<int?> DeductionPayAsYoGoAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        TimeSpan changeValue,
        DateTimeOffset? now = null);

    /// <summary>
    /// 编辑用户会员时长
    /// </summary>
    Task<int> EditUserMembershipAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        Guid? bmUserId,
        DateTimeOffset? endTime,
        TimeSpan? timeSpan,
        string? note);

    Task<Guid?> GetBindPCUserIdAsync(
#if !USE_NUM_UID
        Guid userId,
#else
        long userId,
#endif
        CancellationToken cancellationToken = default);
}

#if !USE_NUM_UID
partial interface IUserMembershipRepository : IRepository<UserMembership, Guid>;
#else
partial interface IUserMembershipRepository : IRepository<UserMembership, long>;
#endif