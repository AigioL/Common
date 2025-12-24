using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户管理
/// </summary>
public static partial class UsersController
{
    const string ControllerName = ControllerConstants.ClientUser;

    public static void MapClientUsers(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/users")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户管理");
    }
}
