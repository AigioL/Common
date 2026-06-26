using AigioL.Common.AspNetCore.AppCenter.Payment.Controllers;
using AigioL.Common.AspNetCore.AppCenter.Payment.Controllers.PayNotify;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册支付服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapPaymentMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b)
        where TAppSettings : class, IAppSettings
    {
        b.MapPaymentAgreement();
        b.MapPaymentCooperatorOrder();
        b.MapPaymentMembership();
        b.MapPayment<TAppSettings>();
        b.MapPaymentTest();
        b.MapPaymentVip();

        b.MapPaymentNotifyAliPay();
        b.MapPaymentNotifyWeChatPayV3();

        b.MapPCUserWithdrawalEndpoints();
    }
}