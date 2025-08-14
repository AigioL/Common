using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台当前登录的后台用户个人资料修改
/// </summary>
public static partial class BMUserController
{
    public static void MapBMUser(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/user")
    {
        var routeGroup = b.MapGroup(pattern);
    }
}
