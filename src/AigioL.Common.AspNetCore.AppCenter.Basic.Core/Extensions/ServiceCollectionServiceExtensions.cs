using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Services;
using AigioL.Common.AspNetCore.AppCenter.Services;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionServiceExtensions
{
    public static IServiceCollection AddAppVerCoreService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IAppVerDbContext
    {
        services.TryAddScoped<IAppVerCoreService, AppVerCoreService<TDbContext>>();
        return services;
    }

    public static IServiceCollection AddBasicRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IArticleDbContext, IOfficialMessageDbContext, IAppVerDbContext
    {
        services.TryAddScoped<IKeyValuePairRepository, KeyValuePairRepository<TDbContext>>();
        services.TryAddScoped<IStaticResourceRepository, StaticResourceRepository<TDbContext>>();

        // Article
        services.TryAddScoped<IArticleCategoryRepository, ArticleCategoryRepository<TDbContext>>();
        services.TryAddScoped<IArticleRepository, ArticleRepository<TDbContext>>();

        // OfficialMessage
        services.TryAddScoped<IOfficialMessageRepository, OfficialMessageRepository<TDbContext>>();

        // AppVer
        services.TryAddScoped<IAppVerBuildRepository, AppVerBuildRepository<TDbContext>>();
        services.TryAddScoped<IAppVerRepository, AppVerRepository<TDbContext>>();
        return services;
    }
}
