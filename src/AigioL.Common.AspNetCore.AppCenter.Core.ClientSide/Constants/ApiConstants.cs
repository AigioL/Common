namespace AigioL.Common.AspNetCore.AppCenter.Constants;

public static partial class ApiConstants
{
    #region HTTP/2 头（全小写）

    /// <summary>
    /// 安全密钥字节数组，使用 Base64Url 编码
    /// </summary>
    public const string Headers_SecurityKey = "app-skey";

    /// <summary>
    /// 安全密钥字节数组，使用 16 进制字符串表示
    /// </summary>
    public const string Headers_SecurityKeyHex = "app-skey-hex";

    /// <summary>
    /// 安全密钥填充
    /// </summary>
    public const string Headers_SecurityKeyPadding = "app-skey-padding";

    /// <summary>
    /// App 是否弃用
    /// </summary>
    public const string Headers_AppObsolete = "app-obsolete";

    #endregion
}
