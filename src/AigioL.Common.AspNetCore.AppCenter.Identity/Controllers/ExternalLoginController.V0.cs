using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class ExternalLoginController
{
    [Obsolete("use MapIdentityExternalLoginV1")]
    public static void MapIdentityExternalLoginV0(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ExternalLogin")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        // TODO: 兼容旧版接口
    }
}