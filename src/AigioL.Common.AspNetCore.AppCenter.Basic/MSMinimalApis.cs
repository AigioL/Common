using AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.AppCenter;

public static partial class MSMinimalApis
{
    /// <summary>
    /// 注册基础服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapBasicMinimalApis(this IEndpointRouteBuilder b)
    {
        b.MapBasicArticle();
        b.MapBasicCustomerService();
    }
}
