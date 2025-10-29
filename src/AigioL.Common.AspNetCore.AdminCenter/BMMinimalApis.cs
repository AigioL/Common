using Microsoft.AspNetCore.Authorization;

namespace AigioL.Common.AspNetCore.AdminCenter;

/// <summary>
/// 管理后台的 WebApi 助手类
/// </summary>
public static partial class BMMinimalApis
{
    /// <summary>
    /// 使用 <see cref="BearerScheme"/> 的授权属性特性实例
    /// </summary>
    public static readonly AuthorizeAttribute ApiControllerBaseAuthorize = new() { AuthenticationSchemes = BearerScheme, };
}

#if !NO_BM_BEARERSCHEME
static partial class BMMinimalApis
{
    /// <summary>
    /// 标识持有者身份验证令牌的方案
    /// </summary>
    public const string BearerScheme = "Bearer";

    /// <summary>
    /// 标识持有者身份验证令牌的方案（小写字母）
    /// </summary>
    public const string BearerSchemeLower = "bearer";
}
#endif