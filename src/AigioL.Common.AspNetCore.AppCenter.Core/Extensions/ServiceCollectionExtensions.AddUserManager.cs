using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Services;
using Microsoft.AspNetCore.Identity;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加由 <see cref="UserManager"/> 实现的用户管理服务
    /// </summary>
    public static IServiceCollection AddACUserManager(this IServiceCollection services)
    {
        services.AddScoped<UserManager<User>, UserManager>();
        return services;
    }
}