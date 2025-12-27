using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 货币汇率管理
/// </summary>
public static partial class ExchangeRateController
{
    const string ControllerName = ControllerConstants.ExchangeRate;

    public static void MapExchangeRate(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/exchangerate")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("货币汇率管理");
    }
}
