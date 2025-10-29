using AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment.PayNotify;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册订单服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapOrderingMinimalApis(
        this IEndpointRouteBuilder b)
    {
        b.MapOrderingAftersalesBill();
        b.MapOrdering();
        b.MapOrderingUserOrder();
    }

    /// <summary>
    /// 注册支付服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapPaymentMinimalApis(
        this IEndpointRouteBuilder b)
    {
        b.MapPaymentAgreement();
        b.MapPaymentCooperatorOrder();
        b.MapPaymentMembership();
        b.MapPayment();
        b.MapPaymentTest();
        b.MapPaymentVip();

        b.MapPaymentNotifyAliPay();
        b.MapPaymentNotifyWeChatPayV3();
    }
}