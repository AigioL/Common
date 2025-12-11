using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using Microsoft.Extensions.Primitives;
using System.Net.Mime;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Helpers.Payment;

public static partial class PaymentHelper
{
    public static PaymentType? GetPaymentTypeFromUserAgent(ReadOnlySpan<char> userAgent)
    {
        if (!userAgent.IsEmpty)
        {
            if (userAgent.Contains("AlipayClient", StringComparison.InvariantCultureIgnoreCase))
            {
                return PaymentType.Alipay;
            }
            else if (userAgent.Contains("MicroMessenger", StringComparison.InvariantCultureIgnoreCase))
            {
                return PaymentType.Alipay;
            }
        }
        return null;
    }

    public static PaymentType? GetPaymentTypeFromUserAgent(StringValues userAgent)
    {
        if (!StringValues.IsNullOrEmpty(userAgent))
        {
            IEnumerable<string> strings = userAgent;
            foreach (var it in strings)
            {
                if (it != null)
                {
                    var paymentType = GetPaymentTypeFromUserAgent(it.AsSpan());
                    if (paymentType.HasValue)
                    {
                        return paymentType.Value;
                    }
                }
            }
        }
        return null;
    }

    public static PaymentType? GetPaymentTypeFromUserAgent(this HttpContext httpContext) => GetPaymentTypeFromUserAgent(httpContext.Request.Headers.UserAgent);

    public static string GetPayErrorPageUrl(string officialUrl, string code)
    {
        var b = new UriBuilder(officialUrl)
        {
            Path = "wechatpay",
            Query = $"error={code}",
        };
        return b.ToString();
    }

    public static IResult RedirectToWechatPayError(HttpContext httpContext, string officialUrl, string code = "1")
    {
        //var paymentType = httpContext.GetPaymentTypeFromUserAgent();

        var contentType = httpContext.Request.ContentType;
        if (contentType != null &&
            contentType.Contains(MediaTypeNames.Application.Json, StringComparison.InvariantCultureIgnoreCase))
        {
            return Results.Json(new TradeAgreementCreateResult(null, code), PaymentMinimalApisJsonSerializerContext.Default.TradeAgreementCreateResult);
        }
        var url = GetPayErrorPageUrl(officialUrl, code);
        return Results.Redirect(url);
    }
}
