using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;

/// <summary>
/// 键值对管理
/// </summary>
public static partial class KeyValuePairController
{
    const string ControllerName = ControllerConstants.KeyValuePair;

    public static void MapKeyValuePair(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/keyvaluepair")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("键值对管理");
    }
}
