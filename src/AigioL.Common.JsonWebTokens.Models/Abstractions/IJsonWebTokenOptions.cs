using Microsoft.IdentityModel.Tokens;
using System.Runtime.Serialization;

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
    /// 签名凭证，用于生成令牌的签名，忽略此属性以避免序列化时暴露敏感信息
    /// </summary>
    [IgnoreDataMember]
    [global::System.Text.Json.Serialization.JsonIgnore]
    SigningCredentials SigningCredentials { get; }
}
