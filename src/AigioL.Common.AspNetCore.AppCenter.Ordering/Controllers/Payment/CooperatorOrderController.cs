using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment;

public static class CooperatorOrderController
{
    public static void MapPaymentCooperatorOrder(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/cooperatororder")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        // TODO: [ServiceFilter(typeof(CooperatorFilterAttribute))]
    }
}
