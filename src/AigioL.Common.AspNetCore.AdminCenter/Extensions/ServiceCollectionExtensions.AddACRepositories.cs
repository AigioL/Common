using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Repositories;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection
{

    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加管理后台的仓储层服务接口
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TRole"></typeparam>
        /// <typeparam name="TUserRole"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddACRepositories<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole>(
            this IServiceCollection services)
            where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>
            where TUser : BMUser
            where TRole : BMRole
            where TUserRole : BMUserRole
        {
            services.AddScoped<IUserManagerExtensions, UserManagerExtensions<TDbContext, TUser, TRole, TUserRole>>();
            services.TryAddScoped<IBMUserRepository, BMUserRepository<TDbContext, TUser, TRole, TUserRole>>();
            services.TryAddScoped<IBMRoleRepository, BMRoleRepository<TDbContext, TUser, TRole, TUserRole>>();
            services.TryAddScoped<IBMMenuRepository, BMMenuRepository<TDbContext, TUser, TRole, TUserRole>>();
            return services;
        }
    }
}

namespace Microsoft.AspNetCore.Http
{
    public static partial class BM_HttpContextExtensions
    {
        /// <summary>
        /// 从 HTTP 上下文中获取管理后台用户 Id
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Guid GetACUserId(this HttpContext ctx)
        {
            var userManager = ctx.RequestServices.GetRequiredService<IUserManagerExtensions>();
            var userId = userManager.GetUserId(ctx);
            return userId;
        }
    }
}

file interface IUserManagerExtensions
{
    Guid GetUserId(HttpContext ctx);
}

file sealed class UserManagerExtensions<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole> :
    IUserManagerExtensions
    where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>
    where TUser : BMUser
    where TRole : BMRole
    where TUserRole : BMUserRole
{
    readonly TDbContext db;

#pragma warning disable IDE0290 // 使用主构造函数
    public UserManagerExtensions(TDbContext db)
#pragma warning restore IDE0290 // 使用主构造函数
    {
        this.db = db;
    }

    public Guid GetUserId(HttpContext ctx)
    {
        var userId = db.GetUserId(ctx);
        if (!userId.HasValue)
        {
#pragma warning disable CA2208 // 正确实例化参数异常
            throw new ArgumentNullException(nameof(userId));
#pragma warning restore CA2208 // 正确实例化参数异常
        }
        return userId.Value;
    }
}
