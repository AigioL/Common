using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.JsonWebTokens.Models.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionServiceExtensions
{
    public static IServiceCollection AddIdentityRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IIdentityDbContext
    {
        services.TryAddScoped<IAuthMessageRecordRepository, AuthMessageRecordRepository<TDbContext>>();
        //services.TryAddScoped<IClockInRecordRepository, ClockInRecordRepository<TDbContext>>();
        services.TryAddScoped<IUserDeleteRepository, UserDeleteRepository<TDbContext>>();
        services.TryAddScoped<IUserMembershipRepository, UserMembershipRepository<TDbContext>>();
        return services;
    }

    /// <summary>
    /// 添加由 <see cref="IUserManager2"/> 实现的用户管理服务
    /// </summary>
    public static IServiceCollection AddACUserManager2<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TOptions : class, IJsonWebTokenOptions
        where TDbContext : DbContext, IIdentityDbContext
    {
        services.AddScoped<IIdentityJsonWebTokenValueProvider, IdentityJsonWebTokenValueProvider<TOptions, TDbContext>>();
        services.AddScoped<UserManager2<TDbContext>>();
        services.AddScoped<IUserManager2>(static s => s.GetRequiredService<UserManager2<TDbContext>>());
        services.AddScoped<UserManager<User>>(static s => s.GetRequiredService<UserManager2<TDbContext>>());
        return services;
    }
}
