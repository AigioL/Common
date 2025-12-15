using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

/// <summary>
/// 用户会员终结点
/// </summary>
public static partial class MembershipController
{
    public static void MapIdentityMembershipV5(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v5/membership")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapPost("v/{tbd}", async (HttpContext context,
            [FromRoute][StringLength(MaxLengths.SHA384)] string tbd) =>
        {
            // tbd 待定传参，保留值
            var r = await GetUserIsMembershipAsync(context, context.RequestAborted);
            return r;
        }).WithDescription("获取用户是否是会员")
        .WithRequiredSecurityKey();
        routeGroup.MapGet("info", async (HttpContext context) =>
        {
            var r = await GetUserMembershipInfoAsync(context, context.RequestAborted);
            return r;
        }).WithDescription("获取会员信息")
        .WithRequiredSecurityKey();
    }

    /// <summary>
    /// 获取用户是否是会员
    /// </summary>
    static async Task<ApiRsp<bool>> GetUserIsMembershipAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var userId = context.GetUserId();
        if (userId == null)
            return ApiRspCode.Unauthorized;

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MembershipController));
        var userMembershipRepo = context.RequestServices.GetRequiredService<IUserMembershipRepository>();
        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

        (MembershipInfo? membershipInfo, var _) = await userMembershipRepo.GetUserMembershipCachePriorityAsync(
            logger,
            connection,
            userId.Value,
            false,
            cancellationToken);

        var isMembership = membershipInfo != null && membershipInfo.IsMembership;
        return new ApiRsp<bool>
        {
            Code = unchecked((uint)ApiRspCode.OK),
            Content = isMembership,
        };
    }

    /// <summary>
    /// 获取会员信息
    /// </summary>
    /// <returns></returns>
    static async Task<ApiRsp<MembershipInfo?>> GetUserMembershipInfoAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var userId = context.GetUserId();
        if (userId == null)
            return ApiRspCode.Unauthorized;

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MembershipController));
        var userMembershipRepo = context.RequestServices.GetRequiredService<IUserMembershipRepository>();
        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

        (MembershipInfo? membershipInfo, bool? lockTake) = await userMembershipRepo.GetUserMembershipCachePriorityAsync(
            logger,
            connection,
            userId.Value,
            true,
            cancellationToken);
        if (lockTake.HasValue && !lockTake.Value)
        {
            return R.操作太频繁请稍后再试;
        }
        return membershipInfo;
    }
}
