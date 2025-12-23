using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 静态资源管理
/// </summary>
public static partial class StaticResourceController
{
    const string ControllerName = ControllerConstants.KeyValuePair;

    public static void MapStaticResource(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/staticresource")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("静态资源管理");
    }
}
