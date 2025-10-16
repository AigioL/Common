using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class MembershipController
{
    [Obsolete("use MapIdentityMembershipV2")]
    public static void MapIdentityMembershipV1(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/membership")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.MSApiControllerBaseAuthorize)
            .WithRequiredSecurityKey();

        routeGroup.MapGet("info", async (HttpContext context) =>
        {
            var r = await GetUserMembershipInfo<MembershipInfoV1>(context);
            return r;
        }).WithDescription("获取会员信息");
    }
}
