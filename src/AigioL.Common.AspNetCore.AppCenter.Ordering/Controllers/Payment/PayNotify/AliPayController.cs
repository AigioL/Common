using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment.PayNotify;

public static class AliPayController
{
    public static void MapPaymentNotifyAliPay(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/notify/alipay")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("", async (HttpContext context) =>
        {
            var r = await UnifiedOrder(context);
            return r;
        }).WithDescription("支付宝应用网关（用于接收由支付宝服务器通知）");
    }

    /// <summary>
    /// 支付宝应用网关（用于接收由支付宝服务器通知）
    /// <para>https://{host}/payment/notify/alipay</para>
    /// </summary>
    static async Task<IResult> UnifiedOrder(
        HttpContext context)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}
