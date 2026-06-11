using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Repositories;

sealed partial class PCUserRepository<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    Repository<TDbContext, TUser, Guid>,
    IPCUserRepository
    where TDbContext : PCDbContextBase<TUser, TRole, TUserRole>
    where TUser : PCUser
    where TRole : PCRole
    where TUserRole : PCUserRole
{
    public PCUserRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public DatabaseFacade Database => db.Database;

    public async Task<PagedModel<BMUserTableItem>> QueryAsync(string? userName, string? nickName, string? name, int current = 1, int pageSize = 10)
    {
        var roles = from ur in db.UserRoles.AsNoTrackingWithIdentityResolution()
                    join role in db.Roles.AsNoTrackingWithIdentityResolution() on ur.RoleId equals role.Id
                    select new
                    {
                        ur.UserId,
                        role.Name,
                    };
        var query = from user in db.Users.AsNoTrackingWithIdentityResolution()
                    select user;
        if (!string.IsNullOrEmpty(userName))
        {
            query = query.Where(x => x.NormalizedUserName!.Contains(userName));
        }
        if (!string.IsNullOrEmpty(nickName))
        {
            query = query.Where(x => x.NickName != null && x.NickName.Contains(nickName));
        }
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(x => x.NormalizedUserName!.Contains(name) || (x.NickName != null && x.NickName.Contains(name)));
        }

        var q1 = query.Select(user => new BMUserTableItem
        {
            Id = user.Id,
            UserName = user.UserName!,
            NickName = user.NickName,
            LockoutEnabled = user.LockoutEnabled,
            Roles = roles.Where(x => x.UserId == user.Id).Select(x => x.Name).ToList(),
        });

        var r = await q1.PagingAsync(current, pageSize, RequestAborted);
        return r;
    }

    public async Task<int> UpdateTenantIdToUserRoleAsync(
        Guid userId,
        Guid tenantId)
    {
        var r = await db.UserRoles
            .Where(x => x.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.TenantId, tenantId)
            , RequestAborted);
        return r;
    }
}
