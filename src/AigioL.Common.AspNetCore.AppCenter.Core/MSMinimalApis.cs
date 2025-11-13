using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter;

/// <summary>
/// 微服务的 WebApi 助手类
/// </summary>
public static partial class MSMinimalApis
{
    /// <summary>
    /// 使用 <see cref="BearerScheme"/> 的授权属性特性实例
    /// </summary>
    public static readonly AuthorizeAttribute ApiControllerBaseAuthorize = new() { AuthenticationSchemes = BearerScheme, };

    static bool Equals(MediaType l, MediaType r) => l.Type == r.Type && l.SubType == r.SubType;

    /// <summary>
    /// 将 Content-Type 或 Accept 头解析为可序列化实现类型
    /// </summary>
    public static bool TryParse(
        StringValues contentTypeOrAccept,
        out SerializableImplType serializableImplType) => TryParse(
            contentTypeOrAccept,
            out var _,
            out serializableImplType,
            out var _,
            out var _);

    /// <summary>
    /// 将 Content-Type 或 Accept 头解析为可序列化实现类型与加密算法类型
    /// </summary>
    public static bool TryParse(
        StringValues contentTypeOrAccept,
        out bool isSecurity,
        out SerializableImplType serializableImplType,
        out SecurityKeyAlgorithmType algorithmType,
        [NotNullWhen(true)] out string? responseContentType)
    {
        isSecurity = false;
        serializableImplType = default;
        algorithmType = default;
        responseContentType = default;
        if (!StringValues.IsNullOrEmpty(contentTypeOrAccept))
        {
            foreach (var it in contentTypeOrAccept)
            {
                if (string.IsNullOrWhiteSpace(it))
                {
                    continue;
                }
                var parsedContentType = new MediaType(it);
                if (Equals(parsedContentType, new(MediaTypeNames.MemoryPack)))
                {
                    isSecurity = false;
                    serializableImplType = SerializableImplType.MemoryPack;
                    responseContentType = MediaTypeNames.MemoryPack;
                    return true;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.MemoryPackSecurity)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.MemoryPack;
                    algorithmType = SecurityKeyAlgorithmType.RSAWithRandomAes;
                    responseContentType = MediaTypeNames.MemoryPackSecurity;
                    return true;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.MemoryPackSecurityECDiffieHellman)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.MemoryPack;
                    algorithmType = SecurityKeyAlgorithmType.ECDHSharedKeyWithRandomIV;
                    responseContentType = MediaTypeNames.MemoryPackSecurityECDiffieHellman;
                    return true;
                }
                //else if (Equals(parsedContentType, new(MediaTypeNames.MessagePack)))
                //{
                //    isSecurity = false;
                //    serializableImplType = SerializableImplType.MessagePack;
                //    return true;
                //}
                //else if (Equals(parsedContentType, new(MediaTypeNames.MessagePackSecurity)))
                //{
                //    isSecurity = true;
                //    serializableImplType = SerializableImplType.MessagePack;
                //    algorithmType = SecurityKeyAlgorithmType.RsaKeyX;
                //    return true;
                //}
                //else if (Equals(parsedContentType, new(MediaTypeNames.MessagePackSecurityECDiffieHellman)))
                //{
                //    isSecurity = true;
                //    serializableImplType = SerializableImplType.MessagePack;
                //    algorithmType = SecurityKeyAlgorithmType.DiffieHellman;
                //    return true;
                //}
                else if (Equals(parsedContentType, new(MediaTypeNames.JSON)))
                {
                    isSecurity = false;
                    serializableImplType = SerializableImplType.SystemTextJson;
                    responseContentType = MediaTypeNames.JSON;
                    return true;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.JSONSecurity)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.SystemTextJson;
                    algorithmType = SecurityKeyAlgorithmType.RSAWithRandomAes;
                    responseContentType = MediaTypeNames.JSONSecurity;
                    return true;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.JSONSecurityECDiffieHellman)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.SystemTextJson;
                    algorithmType = SecurityKeyAlgorithmType.ECDHSharedKeyWithRandomIV;
                    responseContentType = MediaTypeNames.JSONSecurityECDiffieHellman;
                    return true;
                }
            }
        }
        return false;
    }
}

#if !NO_MS_BEARERSCHEME
static partial class MSMinimalApis
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