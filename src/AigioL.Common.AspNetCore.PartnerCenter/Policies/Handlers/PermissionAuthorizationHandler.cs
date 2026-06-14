using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;
using AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.PartnerCenter.Policies.Handlers;

/// <summary>
/// 合作伙伴后台的基于策略的授权
/// <para>https://learn.microsoft.com/zh-cn/aspnet/core/security/authorization/policies#authorization-handlers</para>
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TUser"></typeparam>
/// <typeparam name="TRole"></typeparam>
/// <typeparam name="TUserRole"></typeparam>
public sealed class PermissionAuthorizationHandler<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole>
    : AuthorizationHandler<PermissionAuthorizationRequirementBase>
    where TDbContext : PCDbContextBase<TUser, TRole, TUserRole>
    where TUser : PCUser
    where TRole : PCRole
    where TUserRole : PCUserRole
{
    readonly IServiceProvider serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected sealed override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirementBase requirement)
    {
        var cancellationToken = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.RequestAborted ?? default;
        return HandleRequirementAsync(context, requirement, cancellationToken);
    }

    internal static IQueryable<TUserRole> GetQuery(TDbContext db, Guid userId, string policyName)
    {
        var query = from role in db.UserRoles
                    join user in db.Users on role.UserId equals user.Id
                    join buttonRole in db.MenuButtonRoles on role.RoleId equals buttonRole.RoleId
                    join tenant in db.Tenants on user.TenantId equals tenant.Id
                    where role.UserId == userId
                        && buttonRole.TenantId == user.TenantId
                        && buttonRole.ControllerName == policyName
                        && tenant.DeleteTime == null
                    select role;
#if DEBUG
        var str = query.ToQueryString();
        //Console.WriteLine(str);
#endif
        return query;
    }

    async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirementBase requirement, CancellationToken cancellationToken)
    {
        using var s = serviceProvider.CreateScope();
        var db = s.ServiceProvider.GetRequiredService<TDbContext>();
        var userManager = s.ServiceProvider.GetRequiredService<UserManager<TUser>>();

        if (ShortGuid.TryParse(userManager.GetUserId(context.User), out Guid userId))
        {
            var policyName = requirement.GetPolicyName();
            var query = GetQuery(db, userId, policyName);
            var result = await query.AnyAsync(cancellationToken);
            if (result)
            {
                context.Succeed(requirement);
                return;
            }
        }
        context.Fail();
    }
}
