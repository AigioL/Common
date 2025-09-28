using AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;
using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Services;
using AigioL.Common.AspNetCore.AppCenter.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.AppCenter;

public static partial class MSMinimalApis
{
    /// <summary>
    /// 注册基础服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapBasicMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b)
        where TAppSettings : class, IAppSettings
    {
        b.MapBasicArticle();
        b.MapBasicCustomerService();
        b.MapBasicOfficialMessage();
        b.MapBasicServerCertificateValidate();
        b.MapBasicImage<TAppSettings>();
        b.MapBasicVersions();
    }

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
        return services;
    }
}
