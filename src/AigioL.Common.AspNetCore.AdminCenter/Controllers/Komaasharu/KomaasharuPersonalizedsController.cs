using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Komaasharu;

/// <summary>
/// 广告个性化推荐管理
/// </summary>
[Obsolete("未完善的业务")]
public static partial class KomaasharuPersonalizedsController
{
    const string ControllerName = ControllerConstants.AdvertisementPersonalizedManage;

    public static void MapKomaasharuPersonalizeds(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/komaasharupersonalizedmanage")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("广告个性化推荐管理");

        // 业务设计未完善，暂不实现相关接口
    }
}
