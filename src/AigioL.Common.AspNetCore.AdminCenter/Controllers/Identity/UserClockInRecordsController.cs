using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户签到纪录管理
/// </summary>
public static partial class UserClockInRecordsController
{
    const string ControllerName = ControllerConstants.UserClockInRecord;

    public static void MapUserClockInRecords(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/userclockinrecords")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户签到纪录管理");
    }
}
