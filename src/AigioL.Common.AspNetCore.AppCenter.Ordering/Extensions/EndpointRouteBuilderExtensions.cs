using AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册订单服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapOrderingMinimalApis(
        this IEndpointRouteBuilder b)
    {
        b.MapOrderingAftersalesBill();
        b.MapOrdering();
        b.MapOrderingUserOrder();
    }
}