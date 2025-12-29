using AigioL.Common.AspNetCore.AppCenter.Analytics.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionServiceExtensions
{
    public static IServiceCollection AddAnalyticsRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IAnalysisLogDbContext
    {
        //services.TryAddScoped<IStatisticsRepository, StatisticsRepository<TDbContext>>();
        services.TryAddScoped<IAnalysisLogRepository, AnalysisLogRepository<TDbContext>>();
        return services;
    }
}
