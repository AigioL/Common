using Microsoft.IdentityModel.Tokens;
using System.Buffers;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.JsonWebTokens.Models.Abstractions;

/// <summary>
/// 提供 JsonWebToken 配置项的接口
/// </summary>
public interface IJsonWebTokenOptions
{
    /// <summary>
    /// JsonWebToken 密钥，用于生成和验证令牌
    /// </summary>
    string SecretKey { get; }

    /// <summary>
    /// JsonWebToken 密钥算法，默认使用 <see cref="SecurityAlgorithms.HmacSha384Signature"/>
    /// </summary>
    string SecretAlgorithm { get; }

    /// <summary>
    /// 发行人，用于标识令牌的发行方
    /// </summary>
    string Issuer { get; }

    /// <summary>
    /// 受众，用于指定令牌的受众方
    /// </summary>
    string Audience { get; }

    /// <summary>
    /// 访问令牌过期时间间隔
    /// </summary>
    TimeSpan AccessExpiration { get; }

    /// <summary>
    /// 刷新令牌过期时间间隔
    /// </summary>
    TimeSpan RefreshExpiration { get; }

    /// <summary>
    /// 密钥最小长度
    /// </summary>
    const int SecretKeyMinLength = 16;

    static SecurityKey GetSymmetricSecurityKey(IJsonWebTokenOptions o)
    {
        var key = o.SecretKey;
        var lenKey = Encoding.UTF8.GetMaxByteCount(key.Length);
        if (lenKey < SecretKeyMinLength)
        {
            lenKey = SecretKeyMinLength;
        }
        var buffer = ArrayPool<byte>.Shared.Rent(lenKey);
        try
        {
            var span = buffer.AsSpan(0, lenKey);
            if (!Encoding.UTF8.TryGetBytes(key, span, out var bytesWritten))
            {
                throw new ApplicationException("Failed to convert SecretKey to bytes.");
            }
            if (bytesWritten < span.Length)
            {
                var fillValue = unchecked((byte)'1'); // 填充字符
                span[bytesWritten..].Fill(fillValue);
            }
            //SymmetricSecurityKey2 securityKey = new(span);
            SymmetricSecurityKey securityKey = new(span.ToArray());
            return securityKey;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}


///// <summary>
///// https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/8.13.1/src/Microsoft.IdentityModel.Tokens/SymmetricSecurityKey.cs
///// </summary>
//file sealed class SymmetricSecurityKey2 : SecurityKey
//{
//    readonly int _keySize;
//    readonly string _key;

//    internal SymmetricSecurityKey2(ReadOnlySpan<byte> key)
//        : base()
//    {
//        _key = Base64Url.EncodeToString(key);
//        _keySize = _key.Length * 8;
//    }

//    /// <summary>
//    /// Gets the key size.
//    /// </summary>
//    public override int KeySize => _keySize;

//    ///// <summary>
//    ///// Gets the byte array of the key.
//    ///// </summary>
//    //public virtual byte[] Key
//    //{
//    //    get { return _key.CloneByteArray(); }
//    //}

//    /// <summary>
//    /// Determines whether the <see cref="SymmetricSecurityKey"/> can compute a JWK thumbprint.
//    /// </summary>
//    /// <returns><c>true</c> if JWK thumbprint can be computed; otherwise, <c>false</c>.</returns>
//    /// <remarks>https://datatracker.ietf.org/doc/html/rfc7638</remarks>
//    public override bool CanComputeJwkThumbprint()
//    {
//        return true;
//    }

//    /// <summary>
//    /// Computes a sha256 hash over the <see cref="SymmetricSecurityKey"/>.
//    /// </summary>
//    /// <returns>A JWK thumbprint.</returns>
//    /// <remarks>https://datatracker.ietf.org/doc/html/rfc7638</remarks>
//    public override byte[] ComputeJwkThumbprint()
//    {
//        var canonicalJwk = $@"{{""{JsonWebKeyParameterNames.K}"":""{_key}"",""{JsonWebKeyParameterNames.Kty}"":""{JsonWebAlgorithmsKeyTypes.Octet}""}}";
//        var len = Encoding.UTF8.GetMaxByteCount(canonicalJwk.Length);
//        var buffer = ArrayPool<byte>.Shared.Rent(len);
//        try
//        {
//            var bytesWritten = Encoding.UTF8.GetBytes(canonicalJwk, buffer);
//            var span = buffer.AsSpan(0, bytesWritten);
//            var result = SHA256.HashData(span);
//            return result;
//        }
//        finally
//        {
//            ArrayPool<byte>.Shared.Return(buffer);
//        }
//    }
//}