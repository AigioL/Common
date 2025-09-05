using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment.PayNotify;

public static partial class WeChatPayV3Controller
{
    public static void MapPaymentNotifyWeChatPayV3(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/notify/wechatpay")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("", async (HttpContext context) =>
        {
            var r = await UnifiedOrder(context);
            return r;
        }).WithDescription("微信支付统一下单支付结果通知");
        routeGroup.MapPost("refund", async (HttpContext context) =>
        {
            var r = await Refund(context);
            return r;
        }).WithDescription("微信支付退款结果通知");
        routeGroup.MapPost("agreement", async (HttpContext context) =>
        {
            var r = await HandleAgreementNotice(context);
            return r;
        }).WithDescription("微信支付处理签约/解约结果通知");
        routeGroup.MapPost("v2/agreement", async (HttpContext context) =>
        {
            var r = await HandleAgreementNotice(context);
            return r;
        }).WithDescription("微信支付处理签约/解约结果通知 V2");
    }

    /// <summary>
    /// 微信支付统一下单支付结果通知
    /// <para>https://{host}/payment/notify/wechatpay</para>
    /// </summary>
    static Task<IResult> UnifiedOrder(
        HttpContext context)
    {
        return GetResultAsync(context, "支付结果通知", v2, v3);

        static async Task<IResult> v2(HttpContext context)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        static async Task<IResult> v3(HttpContext context)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 微信支付退款结果通知
    /// <para>https://{host}/payment/notify/wechatpay/refund</para>
    /// </summary>
    static Task<IResult> Refund(
        HttpContext context)
    {
        return GetResultAsync(context, "退款结果通知", v2, v3);

        static async Task<IResult> v2(HttpContext context)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        static async Task<IResult> v3(HttpContext context)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 微信支付处理签约/解约结果通知
    /// <para>https://{host}/payment/notify/wechatpay/agreement</para>
    /// </summary>
    static Task<IResult> HandleAgreementNotice(
        HttpContext context)
    {
        return GetResultAsync(context, "协议签约、解约结果通知", v2, v3);

        static async Task<IResult> v2(HttpContext context)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        static Task<IResult> v3(HttpContext context)
        {
            // UNDONE: 微信还没有 V3 版本的签约/解约通知接口
            return Task.FromResult(WeChatPayNotifyResults.V3.Failure);
        }
    }

    static async Task<IResult> GetResultAsync(
        HttpContext context,
        string notifyType,
        Func<HttpContext, Task<IResult>> v2,
        Func<HttpContext, Task<IResult>> v3)
    {
        bool isV2Request = false;
        try
        {
            var contentType = context.Request.ContentType;
            if (contentType != null && contentType.Contains("text/xml"))
            {
                isV2Request = true;
            }

            if (isV2Request)
            {
                var result = await v2(context);
                return result;
            }
            else
            {
                var result = await v3(context);
                return result;
            }
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetService<ILoggerFactory>()?.CreateLogger(nameof(WeChatPayV3Controller));
            if (logger != null)
            {
                LogError(logger, ex, notifyType);
            }

            if (isV2Request)
            {
                return WeChatPayNotifyResults.V2.Failure;
            }
            else
            {
                return WeChatPayNotifyResults.V3.Failure;
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message =
"""
微信支付通知回调异常
通知类型：{notifyType}
""")]
    static partial void LogError(ILogger logger, Exception exception, string notifyType);
}

/// <summary>
/// WeChatPay 通知应答
/// </summary>
file static class WeChatPayNotifyResults
{
    /// <summary>
    /// WeChatPay V2 通知应答
    /// </summary>
    public static class V2
    {
        public readonly struct SuccessResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteSuccessAsync(httpContext);
        }

        public readonly struct FailureResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteFailureAsync(httpContext);
        }

        public static IResult Success => default(SuccessResult);

        public static IResult Failure => default(FailureResult);

        public static async Task ExecuteSuccessAsync(HttpContext context)
        {
            context.Response.ContentType = "text/xml; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status200OK;
            var w = context.Response.BodyWriter;
            w.Write("<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[SUCCESS]]></return_msg></xml>"u8);
            await w.FlushAsync(context.RequestAborted);
        }

        public static async Task ExecuteFailureAsync(HttpContext context)
        {
            context.Response.ContentType = "text/xml; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var w = context.Response.BodyWriter;
            w.Write("<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[FAIL]]></return_msg></xml>"u8);
            await w.FlushAsync(context.RequestAborted);
        }
    }

    /// <summary>
    /// WeChatPay V3 通知应答
    /// </summary>
    public static class V3
    {
        public readonly struct SuccessResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteSuccessAsync(httpContext);
        }

        public readonly struct FailureResult : IResult
        {
            Task IResult.ExecuteAsync(HttpContext httpContext) => ExecuteFailureAsync(httpContext);
        }

        public static IResult Success => default(SuccessResult);

        public static IResult Failure => default(FailureResult);

        public static async Task ExecuteSuccessAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status200OK;
            var w = context.Response.BodyWriter;
            w.Write("{\"code\":\"SUCCESS\",\"message\":\"SUCCESS\"}"u8);
            await w.FlushAsync(context.RequestAborted);
        }

        public static async Task ExecuteFailureAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var w = context.Response.BodyWriter;
            w.Write("{\"code\":\"FAIL\",\"message\":\"FAIL\"}"u8);
            await w.FlushAsync(context.RequestAborted);
        }
    }
}