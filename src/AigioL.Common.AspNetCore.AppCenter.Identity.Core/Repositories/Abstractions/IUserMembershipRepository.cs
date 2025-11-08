using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IUserMembershipRepository : IRepository<UserMembership, Guid>, IEFRepository
{
    /// <summary>
    /// 增加用户会员订阅类型
    /// </summary>
    Task<bool> AddUserMembershipFlagAsync(Guid userId, MembershipLicenseFlags membershipLicenseFlags);

    /// <summary>
    /// 去除用户指定订阅类型并检查会员是否过期
    /// </summary>
    Task<bool> RemoveUserMembershipFlagAndCheckExpiredAsync(Guid userId, MembershipLicenseFlags membershipLicenseFlags);

    /// <summary>
    /// 获取用户会员信息
    /// </summary>
    Task<MembershipInfo?> GetUserMembershipAsync(Guid userId, CancellationToken cancellationToken = default);
}
