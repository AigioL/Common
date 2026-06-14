using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Controllers.Infrastructure;
using AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册合作伙伴后台的最小 API 路由
    /// </summary>
    public static void MapPCMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDbContext,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRole,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserRole,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b,
        bool enablePwdLogin = true,
        bool enableSmsLogin = true)
        where TDbContext : PCDbContextBase<TUser, TRole, TUserRole>, IPCDbContextBase, IPCDbContext2, IIdentityDbContext<TUser, TRole, Guid, PCUserClaim, TUserRole, PCUserLogin, PCRoleClaim, PCUserToken>, IDbContextBase
        where TUser : PCUser, new()
        where TRole : PCRole, new()
        where TUserRole : PCUserRole
        where TAppSettings : class, IDisableSms
    {
        // Infrastructure
        b.MapPCLogin<TUser, TAppSettings>(enablePwdLogin: enablePwdLogin, enableSmsLogin: enableSmsLogin);
        b.MapPCMenus();
        b.MapPCRoles<TRole>();
        b.MapPCUser<TUser>();
        b.MapPCUsers<TUser>();
    }
}
