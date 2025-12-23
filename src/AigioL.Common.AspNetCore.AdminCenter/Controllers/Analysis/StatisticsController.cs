using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Analysis;

/// <summary>
/// 大数据统计管理
/// </summary>
public static partial class StatisticsController
{
    const string ControllerName = ControllerConstants.StatisticsConsole;

    public static void MapKeyValuePair(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/statistics")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("大数据统计管理");
    }
}
