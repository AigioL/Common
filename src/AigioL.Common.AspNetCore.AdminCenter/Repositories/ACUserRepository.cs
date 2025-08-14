using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
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

    public async Task<PagedModel<ACUserTableItem>> QueryAsync(string? userName, int current = 1, int pageSize = 10)
    {
        var role = from ur in db.UserRoles.AsNoTrackingWithIdentityResolution()
                   join r in db.Roles.AsNoTrackingWithIdentityResolution() on ur.RoleId equals r.Id
                   select new
                   {
                       ur.UserId,
                       r.Name,
                   };
        var query = from user in db.Users.AsNoTrackingWithIdentityResolution()
                    select new ACUserTableItem
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        LockoutEnabled = user.LockoutEnabled,
                        Roles = role.Where(x => x.UserId == user.Id).Select(x => x.Name).ToList(),
                    };
        if (!string.IsNullOrEmpty(userName))
        {
            query = query.Where(x => x.UserName.Contains(userName));
        }
        return await query.PagingAsync(current, pageSize, RequestAborted);
    }
}
