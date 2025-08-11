using Microsoft.IdentityModel.Tokens;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.JsonWebTokens.Models.Abstractions;

/// <summary>
/// 提供 JsonWebToken 配置项的抽象基类
/// </summary>
public abstract class JsonWebTokenOptions : IJsonWebTokenOptions
{
    string? secretKey;
    string? issuer;
    string? audience;
    string? secretAlgorithm;

    public string SecretKey
    {
        get => secretKey ?? throw new InvalidOperationException("SecretKey is not set.");
        set => secretKey = value;
    }

    public string SecretAlgorithm
    {
        get => string.IsNullOrWhiteSpace(secretAlgorithm) ? SecurityAlgorithms.HmacSha384Signature : secretAlgorithm;
        set => secretAlgorithm = value;
    }

    public string Issuer
    {
        get => issuer ?? throw new InvalidOperationException("Issuer is not set.");
        set => issuer = value;
    }

    public string Audience
    {
        get => audience ?? throw new InvalidOperationException("Audience is not set.");
        set => audience = value;
    }

    public TimeSpan AccessExpiration { get; set; }

    public TimeSpan RefreshExpiration { get; set; }

    SigningCredentials? signingCredentials;

    [IgnoreDataMember]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public SigningCredentials SigningCredentials
    {
        get => signingCredentials ?? throw new InvalidOperationException("SigningCredentials is not set.");
        set => signingCredentials = value;
    }

    /// <summary>
    /// 密钥最小长度
    /// </summary>
    const int SecretKeyMinLength = 16;

    public void CreateSigningCredentials()
    {
        if (signingCredentials == null)
        {
            var algorithm = SecretAlgorithm;
            var key = SecretKey;
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
                SymmetricSecurityKey2 securityKey = new(span);
                signingCredentials = new SigningCredentials(securityKey, algorithm);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}

/// <summary>
/// https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/blob/8.13.1/src/Microsoft.IdentityModel.Tokens/SymmetricSecurityKey.cs
/// </summary>
file sealed class SymmetricSecurityKey2 : SecurityKey
{
    readonly int _keySize;
    readonly string _key;

    internal SymmetricSecurityKey2(ReadOnlySpan<byte> key)
        : base()
    {
        _key = Base64Url.EncodeToString(key);
        _keySize = _key.Length * 8;
    }

    /// <summary>
    /// Gets the key size.
    /// </summary>
    public override int KeySize => _keySize;

    ///// <summary>
    ///// Gets the byte array of the key.
    ///// </summary>
    //public virtual byte[] Key
    //{
    //    get { return _key.CloneByteArray(); }
    //}

    /// <summary>
    /// Determines whether the <see cref="SymmetricSecurityKey"/> can compute a JWK thumbprint.
    /// </summary>
    /// <returns><c>true</c> if JWK thumbprint can be computed; otherwise, <c>false</c>.</returns>
    /// <remarks>https://datatracker.ietf.org/doc/html/rfc7638</remarks>
    public override bool CanComputeJwkThumbprint()
    {
        return true;
    }

    /// <summary>
    /// Computes a sha256 hash over the <see cref="SymmetricSecurityKey"/>.
    /// </summary>
    /// <returns>A JWK thumbprint.</returns>
    /// <remarks>https://datatracker.ietf.org/doc/html/rfc7638</remarks>
    public override byte[] ComputeJwkThumbprint()
    {
        var canonicalJwk = $@"{{""{JsonWebKeyParameterNames.K}"":""{_key}"",""{JsonWebKeyParameterNames.Kty}"":""{JsonWebAlgorithmsKeyTypes.Octet}""}}";
        var len = Encoding.UTF8.GetMaxByteCount(canonicalJwk.Length);
        var buffer = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            var bytesWritten = Encoding.UTF8.GetBytes(canonicalJwk, buffer);
            var span = buffer.AsSpan(0, bytesWritten);
            var result = SHA256.HashData(span);
            return result;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}