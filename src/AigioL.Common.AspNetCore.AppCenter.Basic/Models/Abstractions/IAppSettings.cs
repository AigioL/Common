namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;

public interface IAppSettings
{
    /// <summary>
    /// 重定向到目标图片的 Url 地址，格式化字符串，例如 "https://cdn.example.com/images/{0}"
    /// </summary>
    string? ImageUrl { get; }
}
