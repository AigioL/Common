using AigioL.Common.JsonWebTokens.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

/// <summary>
/// 微服务的配置项，使用 UserSecrets 存储值
/// <para>https://learn.microsoft.com/zh-cn/aspnet/core/security/app-secrets</para>
/// </summary>
public class MSAppSettings : JsonWebTokenOptions
{
}
