using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership.MembershipBusinessOrderTableItem;

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
            [FromQuery] Guid? id,
            [FromQuery] string? goodsName,
            [FromQuery] string? goodsNo,
            [FromQuery] MembershipLicenseFlags? memberLicenseType,
            [FromQuery] string? genericOrderId,
            [FromQuery] OrderStatus? paymentStatus,
            [FromQuery] MembershipBusinessSource? businessSource,
            [FromQuery] GoodsRechargeStatus? goodsRechargeStatus,
            [FromQuery] Guid? userId,
            [FromQuery] string? cdkey,
            [FromQuery] string? orderBy,
            [FromQuery] bool? desc,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var paymentTime = context.GetQueryDateTimeRangeNullable("paymentTime");
            var rechargeCompletionTime = context.GetQueryDateTimeRangeNullable("rechargeCompletionTime");
            var membershipBusinessOrderRepo = context.RequestServices.GetRequiredService<IMembershipBusinessOrderRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await membershipBusinessOrderRepo.QueryAsync(
                id, goodsName, goodsNo,
                memberLicenseType, genericOrderId, paymentStatus,
                paymentTime, rechargeCompletionTime, businessSource,
                goodsRechargeStatus, userId, cdkey,
                orderBy, desc, current,
                pageSize, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询会员业务订单");
    }
}
