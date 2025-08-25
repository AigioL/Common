using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AdminCenter.Repositories;

sealed partial class BMRoleRepository<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    Repository<TDbContext, BMMenu, Guid>,
    IBMRoleRepository
    where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>
    where TUser : BMUser
    where TRole : BMRole
    where TUserRole : BMUserRole
{
    public BMRoleRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<PagedModel<BMRoleModel>> QueryAsync(string? name, int current = 1, int pageSize = 10)
    {
        var query = db.Roles.AsNoTrackingWithIdentityResolution();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(x => x.Name!.Contains(name));
        }
        query = query.OrderByDescending(static x => x.CreationTime);

        var q2 = query.OrderByDescending(static x => x.CreationTime)
            .Select(_.RoleExpr);

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

file static class _
{
    internal static readonly Expression<Func<BMRole, BMRoleModel>> RoleExpr = x => new()
    {
        Id = x.Id,
        Name = x.Name!,
    };
}