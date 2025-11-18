using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册身份服务的最小 API 路由
    /// </summary>
    public static void MapIdentityMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings,
        TIdentityDbContext>(
        this IEndpointRouteBuilder b)
        where TAppSettings : class, IDisableSms, IExternalLoginRedirect
        where TIdentityDbContext : IIdentityDbContext
    {
        b.MapIdentityError();

        b.MapIdentityVerificationCodesV5<TAppSettings>();
        b.MapIdentityMembershipV5();
        b.MapIdentityExternalLoginV5<TAppSettings>();
        b.MapIdentityAccountV5<TIdentityDbContext>();
        b.MapIdentityManageV5<TIdentityDbContext>();
    }
}