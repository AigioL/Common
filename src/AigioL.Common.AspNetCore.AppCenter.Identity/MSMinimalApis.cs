using AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

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

#pragma warning disable CS0618 // 类型或成员已过时
        #region V(-1) 版本的接口已经废弃，注册以兼容旧版本客户端 "api/[controller]/[action]"

        b.MapIdentityAuthMessage();
        b.MapIdentityAccount();

        #endregion

        #region V0 版本的接口 "identity/[controller]"

        b.MapIdentityExternalLoginV0();
        b.MapIdentityVerificationCodesV0();

        #endregion

        #region V1 版本的接口 "identity/v1/[controller]"

        b.MapIdentityExternalLoginV1();
        b.MapIdentityAccountV1();
        b.MapIdentityManageV1();
        b.MapIdentityMembershipV1();
        b.MapIdentityVerificationCodesV1();

        #endregion

        #region V2 版本的接口 "identity/v2/[controller]"

        b.MapIdentityAccountV2();

        #endregion
#pragma warning restore CS0618 // 类型或成员已过时
    }
}
