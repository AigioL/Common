using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods to <see cref="IdentityBuilder"/> for adding entity framework stores.
/// </summary>
public static class IdentityEntityFrameworkBuilderExtensions
{
    public static IdentityBuilder AddPartnerCenterEntityFrameworkStores<TContext>(this IdentityBuilder builder)
       where TContext : DbContext, IIdentityDbContext<PCUser, PCRole, Guid, PCUserClaim, PCUserRole, PCUserLogin, PCRoleClaim, PCUserToken>
    {
        // TContext 不继承自 Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<PCUser,...
        // 所以不能直接调用 AddEntityFrameworkStores<TContext>() 方法

        IServiceCollection services = builder.Services;

        services.TryAddScoped<IUserStore<PCUser>, UserStore<PCUser, PCRole, TContext, Guid>>();
        services.TryAddScoped<IRoleStore<PCRole>, RoleStore<PCRole, TContext, Guid>>();

        return builder;
    }
}