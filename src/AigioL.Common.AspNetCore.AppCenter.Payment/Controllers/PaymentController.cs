using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Controllers;

public static class PaymentController
{
    public static void MapPayment<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment")
        where TAppSettings : class, IAppSettings
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("state/{orderId}", async (HttpContext context,
            [FromRoute] string orderId,
            [FromBody] OrderBusinessPaymentMethod method) =>
        {
            var paymentRepo = context.RequestServices.GetRequiredService<IPaymentRepository>();
            var r = await GetOrderPayState(
                paymentRepo, orderId, method,
                context.RequestAborted);
            return r;
        });
        routeGroup.MapPost("{orderId}", async (HttpContext context,
            [FromRoute] string orderId,
            [FromBody] OrderBusinessPaymentMethod method) =>
        {
            var paymentRepo = context.RequestServices.GetRequiredService<IPaymentRepository>();
            var r = await Pay(paymentRepo, context, orderId,
                method, context.RequestAborted);
            return r;
        });
        routeGroup.MapGet("method/{businessType}", async (HttpContext context,
            [FromRoute] int businessType) =>
        {
            var paymentRepo = context.RequestServices.GetRequiredService<IPaymentRepository>();
            var r = await GetMethod(paymentRepo, businessType, context.RequestAborted);
            return r;
        })
        .WithDescription("获取支付方式设置");
        routeGroup.MapGet("redirect", (HttpContext context,
            [FromQuery] string url) =>
        {
            var r = RedirectTo(context, url);
            return r;
        })
        .WithDescription("安全跳转地址，仅允许 https://wx.tenpay.com 开头的地址");
        routeGroup.MapGet("redirect2", (HttpContext context,
            [FromQuery] string url,
            [FromQuery] string code) =>
        {
            var settings = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>().Value;
            var appIdWeChat = settings.WeChatApiOptions.AppId;
            var appSecretWeChat = settings.WeChatApiOptions.AppSecret;
            var r = RedirectToV2(context, appIdWeChat, appSecretWeChat,
                url, code, context.RequestAborted);
            return r;
        });
    }

    static async Task<ApiRsp<bool?>> GetOrderPayState(
        IPaymentRepository paymentRepo,
        string orderId,
        OrderBusinessPaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return ApiRspCode.NotFound;
        }
        var orderPayInfo = await paymentRepo.GetOrderPaymentInfoAsync(
            orderId, true, cancellationToken);
        if (orderPayInfo == null)
        {
            return ApiRspCode.NotFound;
        }
        var r = await paymentRepo.GetPaymentCompositionStateAsync(
            orderPayInfo.Id, method, cancellationToken);
        return r;
    }

    static async Task<ApiRsp<PubPayState?>> Pay(
        IPaymentRepository paymentRepo,
        HttpContext context,
        string orderId,
        OrderBusinessPaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return ApiRspCode.NotFound;
        }
        var orderPayInfo = await paymentRepo.GetOrderPaymentInfoAsync(orderId, true, cancellationToken);
        if (orderPayInfo == null)
        {
            return ApiRspCode.NotFound;
        }
        var isMethodValid = await paymentRepo.IsPaymentMethodValidAsync(orderPayInfo.BusinessType, method, cancellationToken);
        if (isMethodValid)
        {
            var paymentMethod = await paymentRepo.AddOrGetPayMethodAsync(orderPayInfo.Id, orderPayInfo.AmountReceivable, method);
            if (paymentMethod == null)
            {
                return "添加支付方式失败";
            }
            switch (method.PaymentMethod)
            {
                case PaymentMethod.Online:
                    switch (method.PaymentType)
                    {
                        case PaymentType.Alipay:
                            {
                                var aliPayServices = context.RequestServices.GetRequiredService<IAliPayServices>();
                                return await aliPayServices.PubPay(
                                    AliPayPayTradeType.JSAPI_PC,
                                    orderId,
                                    orderPayInfo.OrderNumber,
                                    orderPayInfo.Remarks ?? string.Empty,
                                    orderPayInfo.AmountReceivable,
                                    string.Empty,
                                    orderPayInfo.Timeout);
                            }
                        case PaymentType.WeChatPay:
                            {
                                var weChatPayServices = context.RequestServices.GetRequiredService<IWeChatPayServices>();
                                var remoteIpAddress = context.Connection.RemoteIpAddress;
                                return await weChatPayServices.PubPay(
                                    WeChatPayTradeType.NATIVE,
                                    orderPayInfo.OrderNumber,
                                    orderPayInfo.Remarks ?? string.Empty,
                                    orderPayInfo.AmountReceivable,
                                    string.Empty,
                                    remoteIpAddress?.ToString() ?? string.Empty,
                                    orderPayInfo.Timeout);
                            }
                    }
                    break;
                default:
                    return ApiRspCode.NotFound;
            }
        }
        else
        {
            return "支付方式无效";
        }
        return ApiRspCode.NotFound;
    }

    /// <summary>
    /// 获取支付方式设置
    /// </summary>
    /// <param name="paymentRepo"></param>
    /// <param name="businessType">订单业务类型</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    static async Task<ApiRsp<OrderBusinessPaymentMethod[]?>> GetMethod(
        IPaymentRepository paymentRepo,
        int businessType,
        CancellationToken cancellationToken = default)
    {
        var r = await paymentRepo.GetPaymentMethodAsync(businessType, cancellationToken);
        return r;
    }

    /// <summary>
    /// 安全跳转地址，仅允许 https://wx.tenpay.com 开头的地址
    /// </summary>
    static IResult RedirectTo(
        HttpContext context,
        string url)
    {
        if (url.StartsWith("https://wx.tenpay.com", StringComparison.InvariantCultureIgnoreCase))
        {
            return Results.Redirect(url);
        }
        return Results.NotFound();
    }

    static async Task<IResult> RedirectToV2(
        HttpContext context,
        string appIdWeChat,
        string appSecretWeChat,
        string code,
        string url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(url))
        {
            return Results.NotFound();
        }

        var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var redis = conn.GetDatabase(CacheKeys.RedisAccessTokenDb);

        ReadOnlySpan<char> cache = await redis.HashGetAsync("AccessToken", $"{nameof(PaymentAccessTokenEnum.WeiXinAccessToken)}:{appIdWeChat}");
        var accessToken = JsonSerializer.Deserialize(cache, PaymentMinimalApisJsonSerializerContext.Default.WeChatAccessToken);

        var request = new SnsOAuth2AccessTokenRequest()
        {
            Code = code,
            AccessToken = accessToken!.AccessToken,
            GrantType = "authorization_code",
        };

        var client = new WechatApiClient(new()
        {
            AppId = appIdWeChat,
            AppSecret = appSecretWeChat,
        });

        client.Configure(settings =>
        {
            settings.JsonSerializer = new global::SKIT.FlurlHttpClient.SystemTextJsonSerializer();
        });

        var response = await client.ExecuteSnsOAuth2AccessTokenAsync(request, cancellationToken);
        var redirectUrl = global::Flurl.Url.Parse(url).SetQueryParam("openId", response.OpenId);
        return Results.Redirect(redirectUrl);
    }
}
