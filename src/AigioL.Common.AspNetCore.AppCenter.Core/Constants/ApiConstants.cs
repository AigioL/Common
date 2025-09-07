namespace AigioL.Common.AspNetCore.AppCenter.Constants;

public static partial class ApiConstants
{
    /// <summary>
    /// 安全密钥字节数组，使用 Base64Url 编码
    /// </summary>
    public const string Headers_SecurityKey = "app-skey";

    /// <summary>
    /// 安全密钥字节数组，使用 16 进制字符串表示
    /// </summary>
    public const string Headers_SecurityKeyHex = "app-skey-hex";
}
