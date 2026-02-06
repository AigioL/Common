using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.MerchantDeductionAgreementTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;

/// <summary>
/// 商家扣款协议管理
/// </summary>
public static partial class MerchantDeductionAgreementController
{
    const string ControllerName = ControllerConstants.MerchantDeductionAgreement;

    public static void MapMerchantDeductionAgreement(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/ordering/merchantdeductionagreements")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("商家扣款协议管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? id = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] DateTimeOffset?[]? signingTime = null,
            [FromQuery] DateTimeOffset?[]? unSigningTime = null,
            [FromQuery] PaymentType? platform = null,
            [FromQuery] string? agreementNo = null,
            [FromQuery] string? userOpenId = null,
            [FromQuery] string? userLoginAccount = null,
            [FromQuery] string? extAgreementNo = null,
            [FromQuery] DateTimeOffset? validTime = null,
            [FromQuery] DateTimeOffset? invalidTime = null,
            [FromQuery] string? signScene = null,
            [FromQuery] long? period = null,
            [FromQuery] string? periodType = null,
            [FromQuery] decimal? singleAmount = null,
            [FromQuery] AgreementStatus? status = null,
            [FromQuery] string? note = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int? businessType = null,
            [FromQuery] DateTimeOffset?[]? executeTime = null,
            [FromQuery] DateTimeOffset?[]? initialDeductionTime = null,
            [FromQuery] DateTimeOffset[]? createTime = null,
            [FromQuery] DateTimeOffset[]? updateTime = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var merchantDeductionAgreementRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await merchantDeductionAgreementRepo.QueryAsync(
                id, userId, signingTime,
                unSigningTime, platform, agreementNo,
                userOpenId, userLoginAccount, extAgreementNo,
                validTime, invalidTime, signScene,
                period, periodType, executeTime,
                initialDeductionTime, singleAmount, status,
                createTime, updateTime, note,
                orderBy, desc, businessType,
                current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询商家扣款协议");

        routeGroup.MapPost("{id}/unsign", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            if (id == default)
            {
                return "找不到商家扣款协议";
            }
            var merchantDeductionAgreementRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();
            var agreement = await merchantDeductionAgreementRepo.GetAgreementStatusAndNoAsync(id, context.RequestAborted);
            if (agreement == null)
            {
                return $"找不到商家扣款协议 {id}";
            }
            else if (agreement.Status == AgreementStatus.Terminating)
            {
                return $"商家扣款协议 {id} 正在解约中";
            }
            else if (agreement.Status != AgreementStatus.Signed)
            {
                return $"商家扣款协议 {id} 状态错误";
            }

            var connection = context.RequestServices.GetRequiredService<IConnection>();
            // 发送解约请求
            await CacheKeys.PushAgreementUnSignRequestMessageAsync(connection, agreement.AgreementNo);

            BMApiRsp r = true;
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("解约商家扣款协议");

        //routeGroup.MapPost("{id}/immediatenotify", async (HttpContext context,
        //    [FromRoute] Guid id) =>
        //{
        //    if (id == default)
        //    {
        //        return "找不到商家扣款协议";
        //    }
        //    var merchantDeductionAgreementRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementRepository>();
        //    var agreement = await merchantDeductionAgreementRepo.GetAgreementStatusAndNoAndNoticeStatusAsync(id, context.RequestAborted);
        //    if (agreement == null)
        //    {
        //        return $"找不到商家扣款协议 {id}";
        //    }
        //    else if (agreement.NoticeStatus is not NoticeStatus.WaitNotice and not NoticeStatus.NoticeFail)
        //    {
        //        return $"商家扣款协议 {id} 等待通知第三方或通知第三方失败时不能执行即时通知";
        //    }
        //    else if (agreement.Status == AgreementStatus.Signed)
        //    {
        //        var connection = context.RequestServices.GetRequiredService<IConnection>();
        //        await EnsureNoticeCountAsync(merchantDeductionAgreementRepo, id, agreement.NoticeCount);
        //        await CacheKeys.PushSignAgreementSuccessAsync(connection, agreement.AgreementNo);
        //    }
        //    else if (agreement.Status == AgreementStatus.Terminated)
        //    {
        //        var connection = context.RequestServices.GetRequiredService<IConnection>();
        //        await EnsureNoticeCountAsync(merchantDeductionAgreementRepo, id, agreement.NoticeCount);
        //        await CacheKeys.PushUnSignAgreementSuccessAsync(connection, agreement.AgreementNo);
        //    }
        //    else
        //    {
        //        return $"商家扣款协议 {id} 状态为 {agreement.Status} 时不能执行即时通知";
        //    }

        //    BMApiRsp r = true;
        //    return r;
        //}).PermissionFilter(ControllerName, BMButtonType.Edit)
        //.WithDescription("即时通知商家扣款协议");
    }

    //static async ValueTask EnsureNoticeCountAsync(
    //    IMerchantDeductionAgreementRepository merchantDeductionAgreementRepo,
    //    Guid agreementId,
    //    int noticeCount)
    //{
    //    if (noticeCount >= MerchantDeductionAgreement.MaxNoticeCount)
    //    {
    //        await merchantDeductionAgreementRepo.UpdateNoticeCount(
    //            agreementId,
    //            MerchantDeductionAgreement.MaxNoticeCount - 1);
    //    }
    //}
}
