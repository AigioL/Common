using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 官方消息管理
/// </summary>
public static partial class OfficialMessageController
{
    const string ControllerName = ControllerConstants.OfficialMessage;

    public static void MapOfficialMessage(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/officialmessage")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("官方消息管理");
    }
}
