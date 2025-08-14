using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台角色权限管理
/// </summary>
public static partial class BMRolesController
{
    const string ControllerName = "RoleManage";

    public static void MapBMRoles(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/roles")
    {
        var routeGroup = b.MapGroup(pattern);
    }
}
