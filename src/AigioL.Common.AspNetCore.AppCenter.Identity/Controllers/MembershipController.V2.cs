using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class MembershipController
{
    public static void MapIdentityMembershipV2(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v2/membership")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize)
            .WithRequiredSecurityKey();

        routeGroup.MapGet("info", async (HttpContext context) =>
        {
            var r = await GetUserMembershipInfo<MembershipInfoV2>(context);
            return r;
        }).WithDescription("获取会员信息");
    }
}
