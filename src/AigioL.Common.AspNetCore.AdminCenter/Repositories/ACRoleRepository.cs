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

sealed partial class ACRoleRepository<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUserRole> :
    Repository<TDbContext, ACMenu, Guid>,
    IACRoleRepository
    where TDbContext : ACDbContextBase<TACUser, TACRole, TACUserRole>
    where TACUser : ACUser
    where TACRole : ACRole
    where TACUserRole : ACUserRole
{
    public ACRoleRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<PagedModel<ACRoleModel>> QueryAsync(string? name, int current = 1, int pageSize = 10)
    {
        var query = db.Roles.AsNoTrackingWithIdentityResolution();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(x => x.Name!.Contains(name));
        }
        query = query.OrderByDescending(static x => x.CreationTime);

        var q2 = query.OrderByDescending(static x => x.CreationTime)
            .Select(ACRole.GetExpression());

#if DEBUG
        var sql = q2.ToQueryString();
#endif

        var r = await q2.PagingAsync(current, pageSize, RequestAborted);
        return r;
    }

    public async Task<List<SelectItemModel<Guid>>> GetSelectAsync(int takeCount = 100)
    {
        var query = db.Roles.AsNoTrackingWithIdentityResolution();

        var q2 = query.Select(static x => new SelectItemModel<Guid>
        {
            Id = x.Id,
            Title = x.Name,
        }).Take(takeCount);

        var r = await q2.ToListAsync(RequestAborted);
        return r;
    }

    public async Task<List<Guid>> GetRoleMenus(Guid roleId, Guid? tenantId)
    {
        var query = db.MenuButtonRoles.AsNoTrackingWithIdentityResolution()
            .Where(x => x.RoleId == roleId);
        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId);
        }

        var r = await query.Select(x => x.MenuId).Distinct().ToListAsync(RequestAborted);
        return r;
    }
}
