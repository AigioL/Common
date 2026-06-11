using AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.AspNetCore.PartnerCenter.Repositories;
using AigioL.Common.AspNetCore.PartnerCenter.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection
{

    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加合作伙伴后台的仓储层服务接口
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TRole"></typeparam>
        /// <typeparam name="TUserRole"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPartnerCenterRepositories<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUserRole>(
            this IServiceCollection services)
            where TDbContext : PCDbContextBase<TUser, TRole, TUserRole>
            where TUser : PCUser
            where TRole : PCRole
            where TUserRole : PCUserRole
        {
            services.AddScoped<IUserManagerExtensions, UserManagerExtensions<TDbContext, TUser, TRole, TUserRole>>();
            services.TryAddScoped<IPCUserRepository, PCUserRepository<TDbContext, TUser, TRole, TUserRole>>();
            services.TryAddScoped<IPCRoleRepository, PCRoleRepository<TDbContext, TUser, TRole, TUserRole>>();
            services.TryAddScoped<IPCMenuRepository, PCMenuRepository<TDbContext, TUser, TRole, TUserRole>>();
            return services;
        }
    }
}

namespace Microsoft.AspNetCore.Http
{
    public static partial class PC_HttpContextExtensions
    {
        /// <summary>
        /// 从 HTTP 上下文中获取合作伙伴后台用户 Id
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static Guid GetPCUserId(this HttpContext ctx)
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
    where TDbContext : PCDbContextBase<TUser, TRole, TUserRole>
    where TUser : PCUser
    where TRole : PCRole
    where TUserRole : PCUserRole
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
