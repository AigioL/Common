using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;

/// <summary>
/// 合作方账号管理
/// </summary>
[Obsolete("未完善的业务")]
public static partial class CooperatorAccountController
{
    const string ControllerName = ControllerConstants.CooperatorAccount;

    public static void MapCooperatorAccount(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/ordering/cooperatoraccounts")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("合作方账号管理");
    }
}
