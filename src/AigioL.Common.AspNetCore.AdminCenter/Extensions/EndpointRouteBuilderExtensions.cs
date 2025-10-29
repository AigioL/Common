using AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册管理后台的最小 API 路由
    /// </summary>
    public static void MapBMMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TRole>(this IEndpointRouteBuilder b)
        where TUser : BMUser, new()
        where TRole : BMRole, new()
    {
        b.MapPostInfo();
        b.MapBMLogin<TUser>();
        b.MapBMMenus();
        b.MapBMRoles<TRole>();
        b.MapBMUser<TUser>();
        b.MapBMUsers<TUser>();
    }
}
