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

    /// <inheritdoc/>
    public string SecretKey
    {
        get => secretKey ?? throw new InvalidOperationException("SecretKey is not set.");
        set => secretKey = value;
    }

    /// <inheritdoc/>
    public string SecretAlgorithm
    {
        get => string.IsNullOrWhiteSpace(secretAlgorithm) ? SecurityAlgorithms.HmacSha384Signature : secretAlgorithm;
        set => secretAlgorithm = value;
    }

    /// <inheritdoc/>
    public string Issuer
    {
        get => issuer ?? throw new InvalidOperationException("Issuer is not set.");
        set => issuer = value;
    }

    /// <inheritdoc/>
    public string Audience
    {
        get => audience ?? throw new InvalidOperationException("Audience is not set.");
        set => audience = value;
    }

    public const int DefaultAccessExpirationFromDays = 31;
    public const int DefaultRefreshExpirationFromDays = 62;

    /// <inheritdoc/>
    public virtual TimeSpan AccessExpiration
    {
        get
        {
            if (field == default)
            {
                return TimeSpan.FromDays(DefaultAccessExpirationFromDays);
            }
            return field;
        }
        set => field = value;
    }

    /// <inheritdoc/>
    public virtual TimeSpan RefreshExpiration
    {
        get
        {
            if (field == default)
            {
                return TimeSpan.FromDays(DefaultRefreshExpirationFromDays);
            }
            return field;
        }
        set => field = value;
    }
}