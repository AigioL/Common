using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户注销信息管理
/// </summary>
public static partial class UserCancelsController
{
    const string ControllerName = ControllerConstants.UserCancel;

    public static void MapUserCancels(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/usercancels")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户注销信息管理");
    }
}
