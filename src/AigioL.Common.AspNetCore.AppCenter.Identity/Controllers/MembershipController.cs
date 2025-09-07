using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class MembershipController
{
    public static void MapIdentityMembership(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/membership")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.MSApiControllerBaseAuthorize)
            .WithRequiredSecurityKey();

        //routeGroup.MapPost("loginorregister", async (HttpContext context,
        //    [FromBody] LoginOrRegisterRequest request) =>
        //{
        //    var r = await LoginOrRegister(context, request);
        //    return r;
        //}).WithDescription("登录或注册账号");
    }

    ///// <summary>
    ///// 获取会员信息
    ///// </summary>
    ///// <returns></returns>
    //[HttpGet("info")]
    //public async Task<ApiRspImpl<MembershipInfo?>> GetUserMembershipInfo()
    //{
    //    var user = HttpContext.GetUserId();
    //    if (user == null)
    //        return ApiRspCode.Unauthorized;

    //    var membershipInfo = await GetCacheData(user.Value, connection, () => userMembershipRepository.GetUserMembershipAsync(user.Value));
    //    return membershipInfo;
    //}
}
