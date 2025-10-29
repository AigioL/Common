using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment;

public static class AgreementController
{
    public static void MapPaymentAgreement(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/agreements")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("short/url/{id}", async (HttpContext context,
            [FromRoute] string id) =>
        {
            var r = await ToAgreementSignPageUrl(context, id);
            return r;
        }).AllowAnonymous()
        .WithDescription("短链接");
        routeGroup.MapGet("", async (HttpContext context) =>
        {
            var r = await QueryAsync(context);
            return r;
        }).WithDescription("商家扣款协议列表");
        routeGroup.MapPost("{agreementNo}/unSign", async (HttpContext context,
            [FromRoute] string agreementNo) =>
        {
            var r = await AgreementUnSign(context, agreementNo);
            return r;
        }).WithDescription("商家扣款协议解约");
    }

    /// <summary>
    /// 短链接
    /// </summary>
    /// <param name="context"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    static async Task<IResult> ToAgreementSignPageUrl(
        HttpContext context,
        string id)
    {
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var key = $"AgreementPageUrl:{id}";

        var value = await cache.GetStringAsync(key, context.RequestAborted);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return Results.Redirect(value);
        }
        return Results.NotFound();
    }

    /// <summary>
    /// 商家扣款协议列表
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static async Task<ApiRsp<List<AgreementModel>?>> QueryAsync(
        HttpContext context)
    {
        var userId = context.GetUserIdThrowIfNull();
        var repo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();

        var result = await repo.GetAgreementsByUser(userId);
        return result;
    }

    /// <summary>
    /// 商家扣款协议解约
    /// </summary>
    /// <param name="context"></param>
    /// <param name="agreementNo">协议号</param>
    /// <returns></returns>
    static async Task<ApiRsp> AgreementUnSign(
        HttpContext context,
        string agreementNo)
    {
        var userId = context.GetUserIdThrowIfNull();
        var repo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();

        var agreement = await repo.GetByNo(agreementNo);
        if (agreement == null)
        {
            return "未知的协议号";
        }
        else if (agreement.UserId != userId)
        {
            return "无权操作该协议";
        }
        else if (agreement.Status != AgreementStatus.Signed)
        {
            return "当前状态无法解约";
        }

        var doUnSignAgreement = await repo.DoUnSignAgreement(agreementNo);
        if (doUnSignAgreement)
        {
            var unSignAgreementServices = IUnSignAgreementServices.GetServices(context.RequestServices, agreement.Platform);
            if (unSignAgreementServices == null)
            {
                return "未知的支付平台";
            }
            var unSignAgreement = await unSignAgreementServices.UnSignAgreement(agreement.ExtAgreementNo);
            if (unSignAgreement)
            {
                return HttpStatusCode.OK;
            }
        }
        return "解约失败，请联系管理员";
    }
}
