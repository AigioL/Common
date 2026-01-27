using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;
using AigioL.Common.Models;
using MemoryPack;
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
            [FromQuery] string? openId,
            [FromQuery] string? wxCode,
            [FromBody] OrderBusinessLaunchPaymentMethod method) =>
        {
            var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var paymentRepo = context.RequestServices.GetRequiredService<IPaymentRepository>();
            var r = await Pay<TAppSettings>(paymentRepo, conn, context, orderId,
                method, openId, wxCode, context.RequestAborted);
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

    static async Task<ApiRsp<PubPayState?>> Pay<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        IPaymentRepository paymentRepo,
        IConnectionMultiplexer conn,
        HttpContext context,
        string orderId,
        OrderBusinessLaunchPaymentMethod method,
        string? wxOpenId = null,
        string? wxCode = null,
        CancellationToken cancellationToken = default)
        where TAppSettings : class, IAppSettings
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return ApiRspCode.NotFound;
        }

        // 延迟创建订单模式
        {
            var database = conn.GetDatabase(CacheKeys.RedisMessagingDb);
            var lazyModelValue = await database.StringGetAsync($"OrderIdTemp-{orderId}");
            if (!lazyModelValue.HasValue)
            {
                return ApiRspCode.NotFound;
            }
            var lazyModel = MemoryPackSerializer.Deserialize<LazyCreateMembershipOrderModel>(lazyModelValue);
            if (lazyModel == null)
            {
                return ApiRspCode.NotFound;
            }
            var userMembershipService = context.RequestServices.GetRequiredService<IUserMembershipService>();
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            var goods = await membershipGoodsRepo.FindAsync(lazyModel.MembershipGoodsId, cancellationToken);
            if (goods == null)
            {
                return ApiRspCode.NotFound;
            }
            var orderId2 = await userMembershipService.CreateMembershipOrderAsync(lazyModel.UserId, goods, lazyModel.ChannelPackageId, orderId, cancellationToken);
            if (string.IsNullOrWhiteSpace(orderId2))
            {
                return ApiRspCode.NotFound;
            }
            orderId = orderId2;
        }

        var orderPayInfo = await paymentRepo.GetOrderPaymentInfoAsync(orderId, true, cancellationToken);
        if (orderPayInfo == null)
        {
            return ApiRspCode.NotFound;
        }
        var paymentType = method.PaymentType.ToPaymentType();
        var isMethodValid = await paymentRepo.IsPaymentMethodValidAsync(orderPayInfo.BusinessType, method.PaymentMethod, paymentType, cancellationToken);
        if (isMethodValid)
        {
            var paymentMethod = await paymentRepo.AddOrGetPayMethodAsync(orderPayInfo.Id, orderPayInfo.AmountReceivable, method.PaymentMethod, paymentType);
            if (paymentMethod == null)
            {
                return "添加支付方式失败";
            }
            switch (method.PaymentMethod)
            {
                case PaymentMethod.Online:
                    switch (method.PaymentType)
                    {
                        case WebPaymentType.Alipay:
                        case WebPaymentType.AlipayMWEB:
                            {
                                var payTradeType = method.PaymentType switch
                                {
                                    WebPaymentType.Alipay => AliPayPayTradeType.JSAPI_PC,
                                    WebPaymentType.AlipayMWEB => AliPayPayTradeType.MWEB,
                                    _ => throw new ArgumentOutOfRangeException(nameof(method.PaymentType), method.PaymentType, null),
                                };
                                var aliPayServices = context.RequestServices.GetRequiredService<IAliPayServices>();
                                return await aliPayServices.PubPay(
                                    payTradeType,
                                    orderId,
                                    orderPayInfo.OrderNumber,
                                    orderPayInfo.Remarks ?? string.Empty,
                                    orderPayInfo.AmountReceivable,
                                    string.Empty,
                                    orderPayInfo.Timeout);
                            }
                        case WebPaymentType.WeChatPay:
                        case WebPaymentType.WeChatPayNative:
                            {
                                var weChatPayTradeType = method.PaymentType switch
                                {
                                    WebPaymentType.WeChatPay => WeChatPayTradeType.JSAPI_OFFICIAL,
                                    WebPaymentType.WeChatPayNative => WeChatPayTradeType.NATIVE,
                                    _ => throw new ArgumentOutOfRangeException(nameof(method.PaymentType), method.PaymentType, null),
                                };
                                if (string.IsNullOrWhiteSpace(wxOpenId) && !string.IsNullOrWhiteSpace(wxCode))
                                {
                                    // 微信通过 code 获取 openId
                                    var settings = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>().Value;
                                    var appIdWeChat = settings.WeChatApiOptions.AppId;
                                    var appSecretWeChat = settings.WeChatApiOptions.AppSecret;
                                    var response = await GetAuth2AccessTokenResponse(
                                        wxCode, appIdWeChat, appSecretWeChat,
                                        conn, cancellationToken);
                                    wxOpenId = response.OpenId;
                                }
                                var weChatPayServices = context.RequestServices.GetRequiredService<IWeChatPayServices>();
                                var remoteIpAddress = context.Connection.RemoteIpAddress;
                                return await weChatPayServices.PubPay(
                                    weChatPayTradeType,
                                    orderPayInfo.OrderNumber,
                                    orderPayInfo.Remarks ?? string.Empty,
                                    orderPayInfo.AmountReceivable,
                                    string.Empty,
                                    remoteIpAddress?.ToString() ?? string.Empty,
                                    orderPayInfo.Timeout,
                                    wxOpenId);
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

    /// <summary>
    /// 微信通过 Code 获取 OpenId
    /// </summary>
    static async Task<SnsOAuth2AccessTokenResponse> GetAuth2AccessTokenResponse(
        string code,
        string appIdWeChat,
        string appSecretWeChat,
        IConnectionMultiplexer conn,
        CancellationToken cancellationToken = default)
    {
        var redis = conn.GetDatabase(CacheKeys.RedisAccessTokenDb);

        var weChatAccessTokenRedisValue = await redis.HashGetAsync("AccessToken", $"{nameof(PaymentAccessTokenEnum.WeiXinAccessToken)}:{appIdWeChat}");
        if (!weChatAccessTokenRedisValue.HasValue)
        {
            throw new InvalidOperationException("微信访问令牌不存在，无法获取 OpenId");
        }
        ReadOnlySpan<char> weChatAccessTokenCharsValue = weChatAccessTokenRedisValue;
        if (weChatAccessTokenCharsValue.Length == 0)
        {
            throw new InvalidOperationException("微信访问令牌不正确，无法获取 OpenId");
        }
        var weChatAccessToken = JsonSerializer.Deserialize(weChatAccessTokenCharsValue, PaymentMinimalApisJsonSerializerContext.Default.WeChatAccessToken);
        ArgumentNullException.ThrowIfNull(weChatAccessToken?.AccessToken);
        var request = new SnsOAuth2AccessTokenRequest()
        {
            Code = code,
            AccessToken = weChatAccessToken.AccessToken,
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
        return response;
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
        var response = await GetAuth2AccessTokenResponse(
            code, appIdWeChat, appSecretWeChat,
            conn, cancellationToken);
        var redirectUrl = global::Flurl.Url.Parse(url).SetQueryParam("openId", response.OpenId);
        return Results.Redirect(redirectUrl);
    }
}
