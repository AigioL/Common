using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户设备管理
/// </summary>
public static partial class UserDevicesController
{
    const string ControllerName = ControllerConstants.UserDevice;

    public static void MapUserDevices(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/userdevices")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户设备管理");
    }
}
