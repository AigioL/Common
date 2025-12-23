using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户设备管理
/// </summary>
public static partial class ExternalAccountsController
{
    const string ControllerName = ControllerConstants.ExternalAccount;

    public static void MapExternalAccounts(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/externalaccounts")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户设备管理");
    }
}
