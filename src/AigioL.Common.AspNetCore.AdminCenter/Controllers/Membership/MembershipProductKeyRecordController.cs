using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership.MembershipProductKeyRecordTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Membership;

/// <summary>
/// 会员产品密钥（CDKey）记录管理
/// </summary>
public static partial class MembershipProductKeyRecordController
{
    const string ControllerName = ControllerConstants.MembershipProductKeyRecord;

    public static void MapMembershipProductKeyRecord(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/membership/productkey")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("会员产品密钥（CDKey）记录管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? membershipGoodsId,
            [FromQuery] bool? isUsed,
            [FromQuery] bool? disable,
            [FromQuery] int? rechargeDays,
            [FromQuery][StringLength(MaxLengths.Guid)] string? key,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            Guid? keyGN = ShortGuid.TryParse(key, out Guid keyG) ? keyG : null;
            var membershipProductKeyRecordRepo = context.RequestServices.GetRequiredService<IMembershipProductKeyRecordRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await membershipProductKeyRecordRepo.QueryAsync(
                keyGN, rechargeDays, membershipGoodsId,
                isUsed, disable, orderBy,
                desc, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询会员产品密钥（CDKey）记录");
    }
}
