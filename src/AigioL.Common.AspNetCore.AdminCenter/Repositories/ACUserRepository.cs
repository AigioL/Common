using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Repositories;

sealed partial class ACUserRepository<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUserRole> :
    Repository<TDbContext, ACMenu, Guid>,
    IACUserRepository
    where TDbContext : ACDbContextBase<TACUser, TACRole, TACUserRole>
    where TACUser : ACUser
    where TACRole : ACRole
    where TACUserRole : ACUserRole
{
    public ACUserRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public DatabaseFacade Database => db.Database;

    public async Task<PagedModel<ACUserTableItem>> QueryAsync(string? userName, string? nickName, string? name, int current = 1, int pageSize = 10)
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

        var q1 = query.Select(user => new ACUserTableItem
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
}
