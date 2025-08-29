using AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.AppCenter;

public static partial class MSMinimalApis
{
    /// <summary>
    /// 注册基础服务的最小 API 路由
    /// </summary>
    /// <param name="b"></param>
    public static void MapBasicMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
        this IEndpointRouteBuilder b)
        where TAppSettings : class, IAppSettings
    {
        b.MapBasicArticle();
        b.MapBasicCustomerService();
        b.MapBasicOfficialMessage();
        b.MapBasicServerCertificateValidate();
        b.MapBasicImage<TAppSettings>();
        b.MapBasicVersions();
    }
}
