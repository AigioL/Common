using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment;

public static class PaymentController
{
    public static void MapPayment(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("state/{orderId}", async (HttpContext context,
            [FromRoute] Guid orderId,
            [FromBody] OrderBusinessPaymentMethod method) =>
        {
            var r = await GetOrderPayState(context, orderId, method);
            return r;
        });
        routeGroup.MapPost("{orderId}", async (HttpContext context,
            [FromRoute] Guid orderId,
            [FromBody] OrderBusinessPaymentMethod method) =>
        {
            var r = await Pay(context, orderId, method);
            return r;
        });
        routeGroup.MapGet("method/{businessType}", async (HttpContext context,
            [FromRoute] int businessType) =>
        {
            var r = await GetMethod(context, businessType);
            return r;
        })
        .WithDescription("获取支付方式设置");
        routeGroup.MapGet("redirect", (HttpContext context,
            [FromQuery] string url) =>
        {
            var r = RedirectTo(context, url);
            return r;
        });
        routeGroup.MapGet("redirect2", (HttpContext context,
            [FromQuery] string url,
            [FromQuery] string code) =>
        {
            var r = RedirectToV2(context, url, code);
            return r;
        });
    }

    static async Task<ApiRsp<bool?>> GetOrderPayState(
        HttpContext context,
        Guid orderId,
        OrderBusinessPaymentMethod method)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (orderId == default)
        //    return ApiRspCode.NotFound;
        //var orderPayInfo = await paymentRepo.GetOrderPaymentInfo(orderId, true);
        //if (orderPayInfo == null)
        //    return ApiRspCode.NotFound;
        //return await paymentRepo.GetPaymentCompositionState(orderPayInfo.Id, method);
    }

    static async Task<ApiRsp<PubPayState?>> Pay(
        HttpContext context,
        Guid orderId,
        OrderBusinessPaymentMethod method)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (orderId == default)
        //    return ApiRspCode.NotFound;
        //var orderPayInfo = await paymentRepo.GetOrderPaymentInfo(orderId, true);
        //if (orderPayInfo == null)
        //    return ApiRspCode.NotFound;
        //var isMethodValid = await paymentRepo.IsPaymentMethodValid(orderPayInfo.BusinessType, method);
        //if (isMethodValid)
        //{
        //    var paymentMethod = await paymentRepo.AddOrGetPayMethod(orderPayInfo.Id, orderPayInfo.AmountReceivable, method);
        //    if (paymentMethod == null)
        //        return "添加支付方式失败";
        //    switch (method.PaymentMethod)
        //    {
        //        case PaymentMethod.Online:
        //            switch (method.PaymentType)
        //            {
        //                case PaymentType.Alipay:
        //                    return await aliPayServices.PubPay(
        //                        AliPayPayTradeType.JSAPI_PC,
        //                        orderId,
        //                        orderPayInfo.OrderNumber,
        //                        orderPayInfo.Remarks ?? string.Empty,
        //                        orderPayInfo.AmountReceivable,
        //                        string.Empty,
        //                        orderPayInfo.Timeout);

        //                case PaymentType.WeChatPay:
        //                    var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
        //                    return await weChatPayServices.PubPay(
        //                        WeChatPayTradeType.NATIVE,
        //                        orderPayInfo.OrderNumber,
        //                        orderPayInfo.Remarks ?? string.Empty,
        //                        orderPayInfo.AmountReceivable,
        //                        string.Empty,
        //                        remoteIpAddress?.ToString() ?? string.Empty,
        //                        orderPayInfo.Timeout);
        //            }
        //            break;

        //        default:
        //            return ApiRspCode.NotFound;
        //    }
        //}
        //else
        //{
        //    return "支付方式无效";
        //}
        //return ApiRspCode.NotFound;
    }

    /// <summary>
    /// 获取支付方式设置
    /// </summary>
    /// <param name="context"></param>
    /// <param name="businessType">订单业务类型</param>
    /// <returns></returns>
    static async Task<ApiRsp<OrderBusinessPaymentMethod[]?>> GetMethod(
        HttpContext context,
        int businessType)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //return await paymentRepo.GetPaymentMethod(businessType);
    }

    static IResult RedirectTo(
        HttpContext context,
        string url)
    {
        throw new NotImplementedException();
        //if (url.StartsWith("https://wx.tenpay.com/"))
        //    return Redirect(url);
        //return NotFound();
    }

    static async Task<IResult> RedirectToV2(
        HttpContext context,
        string code,
        string url)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(url))
        //    return NotFound();

        //var redis = connection.GetDatabase(CacheKeys.RedisAccessTokenDb);

        //var cache = await redis.HashGetAsync("AccessToken", AccessTokenEnum.WeiXinAccessToken.ToString() + ":" + appSettings!.WeChatApiOptions!.AppId);
        //var accessToken = Newtonsoft.Json.JsonConvert.DeserializeObject<WeChatAccessToken>(cache.ToString());

        //var request = new SKIT.FlurlHttpClient.Wechat.Api.Models.SnsOAuth2AccessTokenRequest()
        //{
        //    Code = code,
        //    AccessToken = accessToken!.AccessToken,
        //    GrantType = "authorization_code",
        //};

        //var client = new WechatApiClient(new()
        //{
        //    AppId = appSettings!.WeChatApiOptions!.AppId!,
        //    AppSecret = appSettings!.WeChatApiOptions!.AppSecret!,
        //});

        //client.Configure(settings =>
        //{
        //    settings.JsonSerializer = new SKIT.FlurlHttpClient.NewtonsoftJsonSerializer();
        //});

        //var response = await client.ExecuteSnsOAuth2AccessTokenAsync(request, HttpContext.RequestAborted);
        //var flurl = Flurl.Url.Parse(url).SetQueryParam("openId", response.OpenId);

        //return Redirect(flurl);
    }
}
