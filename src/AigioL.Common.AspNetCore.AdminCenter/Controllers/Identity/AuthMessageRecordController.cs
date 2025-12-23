using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 验证码记录管理
/// </summary>
public static partial class AuthMessageRecordController
{
    const string ControllerName = ControllerConstants.AuthMessageRecord;

    public static void MapAuthMessageRecord(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/authmessagerecord")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("验证码记录管理");
    }
}
