using AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.AppCenter;

public static partial class MSMinimalApis
{
    /// <summary>
    /// 注册身份服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapIdentityMinimalApis(
        this IEndpointRouteBuilder b)
    {
        b.MapIdentityError();
        b.MapIdentityExternalLogin();
        b.MapIdentityAccount();
        b.MapIdentityManage();
    }
}
