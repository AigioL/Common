using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment;

public static class VipPaymentController
{
    public static void MapPaymentVip(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/vip")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("PayLink", async (HttpContext context,
            [FromQuery] Guid goodId) =>
        {
            var r = await PayLink(context, goodId);
            return r;
        }).WithDescription("支付页链接");
        routeGroup.MapGet("Order/{payId}", async (HttpContext context,
            [FromRoute] string payId) =>
        {
            var r = await Order(context, payId);
            return r;
        }).AllowAnonymous()
        .WithDescription("创建订单并跳转到支付页面（此链接由用户的微信或支付宝打开）");
        routeGroup.MapGet("PayResult", async (HttpContext context,
            [FromQuery] string payId) =>
        {
            var r = await PayResult(context, payId);
            return r;
        }).WithDescription("获取支付结果");
    }

    ///// <summary>
    ///// 获取商品列表
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet(nameof(Goods))]
    //public async Task<ApiRspImpl<List<XunYouGoodDTO>?>> Goods()
    //    => await redisDb.GetOrCreateAsync("XunYouGoods", xunYouService.GetGoods);

    /// <summary>
    /// 支付页链接
    /// </summary>
    /// <returns>创建订单的链接，通常是以二维码的方式发送到用户手机去支付</returns>
    static async Task<ApiRsp<string?>> PayLink(
        HttpContext context,
        Guid goodId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //var userId = HttpContext.GetUserId();
        //if (userId == null)
        //    return ApiRspHelper.Fail<string>("获取用户失败");

        //var jwtUserId = HttpContext.GetJwtUserId()?.ToStringS();

        //var good = await xunYouService.GetGoodDTOById(goodId);
        //if (good == null)
        //    return ApiRspHelper.Fail<string>("获取商品失败");

        //var orderNumber = xunYouService.NewOutTradeNo();
        //var agreementNo = good.IsAgreement ? xunYouService.NewAgreementNo() : null;

        //// 生成“支付Id”，它记录用户和商品的信息。
        //var payId = PayId.NewPayId();
        //var payInfo = new PaymentInfo(payId.ToString(), userId.Value, goodId, orderNumber, agreementNo, jwtUserId);

        //// 将信息存入缓存
        //await redisDb.AddAsync(PaymentInfo.GetKeyOfPayId(payId), payInfo, cacheDuration);
        //await redisDb.AddAsync(PaymentInfo.GetKeyOfOrderNumber(orderNumber), payInfo, cacheDuration);

        //// 返回下单链接，客户端二维码显示
        //// 类似：https://api.stampp.net/payment/order/1723631354-6db3ce9db88447fe91bb37666b2050da
        //var orderingUrl = new Flurl.Url(appSettings.ApiUrl).AppendPathSegment($"{apiRoute}/{nameof(Order)}/{payId}");
        //return ApiRspHelper.Ok(orderingUrl.ToString());
    }

    /// <summary>
    /// 创建订单并跳转到支付页面（此链接由用户的微信或支付宝打开）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="payId">支付 Id</param>
    /// <returns></returns>
    static async Task<IResult> Order(
        HttpContext context,
        string payId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //var userDeviceIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        //if (string.IsNullOrEmpty(userDeviceIp))
        //    return paymentHelper.RedirectToWechatPayError();

        //var paymentType = paymentHelper.GetPaymentTypeFromUserAgent();
        //if (paymentType is not PaymentType.Alipay and not PaymentType.WeChatPay)
        //    return paymentHelper.RedirectToWechatPayError(XunYouReturnCode.PaymentTypeNotSupported.Code);

        //if (await weChatHelper.EnsureUserWechatOpenId() is var (redirect, userWechatOpenId) && redirect != null)
        //    return redirect;
        //if (paymentType == PaymentType.WeChatPay && userWechatOpenId == null)
        //    return paymentHelper.RedirectToWechatPayError();
        //else
        //    userWechatOpenId ??= "";

        //if (!PayId.TryParse(payId, out var parsedPayId))
        //    return paymentHelper.RedirectToWechatPayError();
        //if (DateTime.Now - parsedPayId.Timestamp >= paymentDuration)
        //    return paymentHelper.RedirectToWechatPayError(XunYouReturnCode.PaymentTimeout.Code); // "操作超时，请重试"

        //var payInfo = await redisDb.GetAsync<PaymentInfo>(PaymentInfo.GetKeyOfPayId(payId));
        //if (payInfo == null)
        //    return paymentHelper.RedirectToWechatPayError();

        //var good = await xunYouService.GetGoodDTOById(payInfo.GoodId);
        //if (good == null)
        //    return paymentHelper.RedirectToWechatPayError(XunYouReturnCode.ConfigurationInvalid.Code);

        //// 检查商品类型
        //// 创建订单或协议并跳转到支付页面
        //if (!good.IsAgreement)
        //    return await CreateOrder(good, payInfo, paymentType.Value, userWechatOpenId, userDeviceIp);
        //else
        //    return await CreateAgreement(good, payInfo, paymentType.Value, userWechatOpenId, userDeviceIp);
    }

    /// <summary>
    /// 获取支付结果
    /// </summary>
    /// <param name="context"></param>
    /// <param name="payId"></param>
    /// <returns></returns>
    static async Task<ApiRsp<OrderStatus?>> PayResult(
        HttpContext context,
        string payId)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (!PayId.TryParse(payId, out var parsedPayId))
        //    return "参数错误";
        //var payInfoExpireTime = parsedPayId.Timestamp.Add(cacheDuration);
        //if (DateTime.Now >= payInfoExpireTime)
        //    return "操作超时，请重试";
        //var payInfo = await redisDb.GetAsync<PaymentInfo>(PaymentInfo.GetKeyOfPayId(payId));
        //if (payInfo == null)
        //    return "参数错误";

        //var orderNumber = payInfo.OrderNumber;

        //// 查询订单状态，如果不是“待支付”，则直接返回它。
        //// 查询不到也算未支付，因为用户可能还未扫码创建订单。
        //var orderStatus = await xunYouService.GetOrderStatus(orderNumber) ?? OrderStatus.WaitPay;
        //if (orderStatus != OrderStatus.WaitPay)
        //    return orderStatus;

        //try
        //{
        //    // 订阅订单状态变化
        //    var channel = await connection.GetSubscriber().SubscribeAsync(RedisChannel.Literal($"{orderNumber}:OrderStatus"));
        //    var message = await channel.ReadAsync(new CancellationTokenSource(payInfoExpireTime - DateTime.Now).Token);
        //    await channel.UnsubscribeAsync();

        //    // 返回最新订单状态
        //    if (message.Message.TryParse(out int status))
        //        return (OrderStatus)status;
        //}
        //catch (TaskCanceledException) { }
        //catch (Exception) { }

        //// 如果超时或异常，再次查询最新订单状态并返回
        //return await xunYouService.GetOrderStatus(orderNumber);
    }
}
