using AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

namespace AigioL.Common.AspNetCore.AdminCenter;

/// <summary>
/// 管理后台（Admin Center）的最小 API
/// </summary>
public static partial class ACMinimalApis
{
    public static void MapACMinimalApis(this IEndpointRouteBuilder b)
    {
        b.MapPostInfo();

        b.MapGetIpV6();
        b.MapGetIpVal();
    }
}
