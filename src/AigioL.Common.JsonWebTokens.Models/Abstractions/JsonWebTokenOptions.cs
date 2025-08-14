using Microsoft.IdentityModel.Tokens;

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

    public virtual TimeSpan AccessExpiration { get; set; }

    public virtual TimeSpan RefreshExpiration { get; set; }
}