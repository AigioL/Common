using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Repositories;
using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Services;
using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddPCUserRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IIdentityDbContext<PCUser, PCRole, Guid, PCUserClaim, PCUserRole, PCUserLogin, PCRoleClaim, PCUserToken>, IPCDbContext2
    {
        services.TryAddScoped<IPCUserRepository, PCUserRepository<TDbContext>>();
        return services;
    }

    /// <summary>
    /// 添加 PC 用户配置相关服务
    /// </summary>
    public static IServiceCollection AddPCUserConfigServices<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IPCDbContext2
    {
        services.TryAddScoped<IPCUserConfigService, PCUserConfigService<TDbContext>>();
        return services;
    }
}
