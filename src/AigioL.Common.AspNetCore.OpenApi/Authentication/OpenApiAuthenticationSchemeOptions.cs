using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;

namespace AigioL.Common.AspNetCore.OpenApi.Authentication;

public sealed partial class OpenApiAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// 支持的 HMAC 哈希算法类型名称列表，如果为 null，则表示支持所有安全的 HMAC 哈希算法类型名称
    /// </summary>
    public HashAlgorithmTypeName[]? SupportedHMACHashAlgorithms { get; set; }

    /// <summary>
    /// 是否为不支持的 HMAC 哈希算法类型名称
    /// </summary>
    public bool IsUnSupported(HashAlgorithmTypeName hashAlgorithmTypeName)
    {
        var supportedHMACHashAlgorithms = SupportedHMACHashAlgorithms;
        if (supportedHMACHashAlgorithms == null)
        {
            switch (hashAlgorithmTypeName)
            {
                case HashAlgorithmTypeName.SHA256:
                    break;
                case HashAlgorithmTypeName.SHA384:
                    break;
                case HashAlgorithmTypeName.SHA512:
                    break;
                default:
                    return true;
            }
            return false;
        }
        return !supportedHMACHashAlgorithms.Contains(hashAlgorithmTypeName);
    }
}
