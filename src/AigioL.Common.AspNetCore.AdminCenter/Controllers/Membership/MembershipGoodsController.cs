using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership.MembershipGoodsTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Membership;

/// <summary>
/// 会员商品管理
/// </summary>
public static partial class MembershipGoodsController
{
    const string ControllerName = ControllerConstants.MembershipGoods;

    public static void MapMembershipGoods(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/membership/goods")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("会员商品管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? id,
            [FromQuery] string? goodsName,
            [FromQuery] string? goodsNo,
            [FromQuery] MembershipLicenseFlags? memberLicenseType,
            [FromQuery] int? rechargeDays,
            [FromQuery] decimal? currentPrice,
            [FromQuery] bool? enable,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await membershipGoodsRepo.QueryAsync(
                id, goodsName, goodsNo,
                memberLicenseType, rechargeDays, currentPrice,
                enable, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询会员商品");
    }
}
