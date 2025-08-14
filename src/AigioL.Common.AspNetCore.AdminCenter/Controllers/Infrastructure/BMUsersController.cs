using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台的用户管理
/// </summary>
public static partial class BMUsersController
{
    const string ControllerName = "SystemUser";

    public static void MapBMUsers(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/users")
    {
        var routeGroup = b.MapGroup(pattern).RequireAuthorization(new AuthorizeAttribute()
        {
            AuthenticationSchemes = BMLoginController.BearerScheme,
        });

        //routeGroup.RequireAuthorization(new PermissionAuthorizationRequirement(ControllerName, ACButtonType.Add));
    }
}
