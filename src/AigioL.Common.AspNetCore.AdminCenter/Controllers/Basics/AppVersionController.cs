using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 客户端版本管理
/// </summary>
public static partial class AppVersionController
{
    const string ControllerName = ControllerConstants.AppVersion;

    public static void MapAppVersion(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/appversion")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端版本管理");
    }
}
