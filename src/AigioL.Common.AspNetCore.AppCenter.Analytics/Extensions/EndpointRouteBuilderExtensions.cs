using AigioL.Common.AspNetCore.AppCenter.Analytics.Controllers;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册分析服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapAnalyticsMinimalApis(
        this IEndpointRouteBuilder b)
    {
        b.MapAnalyticsLogs();
        b.MapAnalyticsActiveUsers();
    }
}
