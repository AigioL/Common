using AigioL.Common.JsonWebTokens.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 管理后台的配置项，使用 UserSecrets 存储值
/// <para>https://learn.microsoft.com/zh-cn/aspnet/core/security/app-secrets</para>
/// </summary>
public abstract class ACAppSettings : JsonWebTokenOptions
{
    /// <summary>
    /// 用于创建一个默认管理员账号的用户名
    /// </summary>
    public virtual string? AdminUserName { get; set; }

    /// <summary>
    /// 用于创建一个默认管理员账号的密码
    /// </summary>
    public virtual string? AdminPassword { get; set; }

    /// <summary>
    /// 初始化后台系统的哈希盐值
    /// </summary>
    public virtual string? InitSystemSecuritySalt { get; set; }

    /// <summary>
    /// 管理后台的 RSA 私钥
    /// </summary>
    public virtual byte[]? AdminRSAPrivateKey { get; set; }

    public const int DefaultAccessExpirationFromDays = 31;
    public const int DefaultRefreshExpirationFromDays = 62;

    public override TimeSpan AccessExpiration
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

    public override TimeSpan RefreshExpiration
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
