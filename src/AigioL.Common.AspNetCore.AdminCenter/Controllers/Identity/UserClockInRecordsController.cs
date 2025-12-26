using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户签到纪录管理
/// </summary>
[Obsolete("未完善的业务")]
public static partial class UserClockInRecordsController
{
    const string ControllerName = ControllerConstants.UserClockInRecord;

    public static void MapACUserClockInRecords(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/userclockinrecords")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("客户端用户签到纪录管理");
    }
}
