using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Controllers;

public static class CooperatorOrderController
{
    const string RedisKey_WechatPaymentRequest = "CreateWechatPaymentRequest:";
    const string RedisKey_AgreementSignDeductRequest = "CreateWechatAgreementSignDeduct:";

    public static void MapPaymentCooperatorOrder(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/cooperatororder")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        // TODO: [ServiceFilter(typeof(CooperatorFilterAttribute))]
    }
}
