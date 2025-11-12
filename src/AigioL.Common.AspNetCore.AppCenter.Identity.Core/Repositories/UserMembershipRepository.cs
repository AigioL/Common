using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class UserMembershipRepository<TDbContext> :
    Repository<TDbContext, UserMembership, Guid>,
    IUserMembershipRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserMembershipRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

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
                     }).Take(1);
        var r = query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }
}