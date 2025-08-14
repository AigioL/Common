using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Policies.Handlers;

/// <summary>
/// 基于策略的授权
/// <para>https://learn.microsoft.com/zh-cn/aspnet/core/security/authorization/policies#authorization-handlers</para>
/// </summary>
/// <typeparam name="TACDbContext"></typeparam>
/// <typeparam name="TACUser"></typeparam>
/// <typeparam name="TACRole"></typeparam>
/// <typeparam name="TACUserRole"></typeparam>
public sealed class PermissionAuthorizationHandler<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TACUserRole>
    : AuthorizationHandler<PermissionAuthorizationRequirement>
    where TACDbContext : ACDbContextBase<TACUser, TACRole, TACUserRole>
    where TACUser : ACUser
    where TACRole : ACRole
    where TACUserRole : ACUserRole
{
    readonly IServiceProvider serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected sealed override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
    {
        var cancellationToken = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.RequestAborted ?? default;
        return HandleRequirementAsync(context, requirement, cancellationToken);
    }

    internal static IQueryable<TACUserRole> GetQuery(TACDbContext db, Guid userId, string policyName)
    {
        var query = from role in db.UserRoles
                    join user in db.Users on role.UserId equals user.Id
                    join buttonRole in db.MenuButtonRoles on role.RoleId equals buttonRole.RoleId
                    join tenant in db.Tenants on user.TenantId equals tenant.Id
                    where role.UserId == userId &&
                        buttonRole.TenantId == user.TenantId &&
                        buttonRole.ControllerName == policyName &&
                        !tenant.SoftDeleted
                    select role;
#if DEBUG
        var str = query.ToQueryString();
        //Console.WriteLine(str);
#endif
        return query;
    }

    async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement, CancellationToken cancellationToken)
    {
        using var s = serviceProvider.CreateScope();
        var db = s.ServiceProvider.GetRequiredService<TACDbContext>();
        var userManager = s.ServiceProvider.GetRequiredService<UserManager<TACUser>>();

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
