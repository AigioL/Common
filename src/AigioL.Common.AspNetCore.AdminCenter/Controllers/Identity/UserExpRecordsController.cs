using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户经验值记录管理
/// </summary>
[Obsolete("未完善的业务")]
public static partial class UserExpRecordsController
{
    const string ControllerName = ControllerConstants.UserExpRecord;

    public static void MapACUserExpRecords(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/authmessagerecord")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户经验值记录管理");
    }
}
