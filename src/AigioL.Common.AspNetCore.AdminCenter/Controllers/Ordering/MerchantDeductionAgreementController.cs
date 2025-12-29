using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
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
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var executeTime = context.GetQueryDateTimeRangeNullable("executeTime");
            var initialDeductionTime = context.GetQueryDateTimeRangeNullable("initialDeductionTime");
            var createTime = context.GetQueryDateTimeRange("createTime");
            var updateTime = context.GetQueryDateTimeRange("updateTime");
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
    }
}
