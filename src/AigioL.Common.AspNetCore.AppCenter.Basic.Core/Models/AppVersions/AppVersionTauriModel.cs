using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;

/// <summary>
/// https://tauri.org.cn/v1/guides/distribution/updater/#dynamic-update-server
/// <para>
/// {
///   "version": "0.2.0",
///   "pub_date": "2020-09-18T12:29:53+01:00",
///   "url": "https://mycompany.example.com/myapp/releases/myrelease.tar.gz",
///   "signature": "Content of the relevant .sig file",
///   "notes": "These are some release notes"
/// }
/// </para>
/// </summary>
public sealed record class AppVersionTauriModel
{
    /// <summary>
    /// 必须是有效的 semver，可以带或不带前导 v，这意味着 1.0.0 和 v1.0.0 都是有效的。
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; set; }

    /// <summary>
    /// 如果存在，"pub_date" 必须按照 RFC 3339 进行格式化。
    /// </summary>
    [JsonPropertyName("pub_date")]
    public DateTimeOffset? PublishTime { get; set; }

    /// <summary>
    /// 必须是更新包的有效 URL。
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// 必须是生成的 .sig 文件的内容。每次运行 tauri build 时签名都可能发生变化，因此请务必始终对其进行更新。
    /// </summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    /// <summary>
    /// 此处可以添加有关更新的说明，例如发行说明。Tauri 的默认对话框会在询问是否允许更新时向用户显示此说明。
    /// </summary>
    [JsonPropertyName("notes")]
    public required string Notes { get; set; }
}

[JsonSerializable(typeof(AppVersionTauriModel))]
[JsonSourceGenerationOptions]
public sealed partial class AppVersionTauriModelJsonSerializerContext : JsonSerializerContext
{
    static AppVersionTauriModelJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new AppVersionTauriModelJsonSerializerContext(o);
    }
}
