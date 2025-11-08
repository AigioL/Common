using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.Models;
using System.Diagnostics.CodeAnalysis;

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
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize)
            .WithRequiredSecurityKey();

        routeGroup.MapGet("info", async (HttpContext context) =>
        {
            var r = await GetUserMembershipInfo(context);
            return r;
        }).WithDescription("获取会员信息");
    }

    /// <summary>
    /// 获取会员信息
    /// </summary>
    /// <returns></returns>
    static async Task<ApiRsp<MembershipInfo?>> GetUserMembershipInfo(
        HttpContext context)
    {
        await Task.CompletedTask;
        //var user = HttpContext.GetUserId();
        //if (user == null)
        //    return ApiRspCode.Unauthorized;

        //var membershipInfo = await GetCacheData(user.Value, connection, () => userMembershipRepository.GetUserMembershipAsync(user.Value));
        //return membershipInfo;
        return "TODO: 待完成";
    }
}
