using AigioL.Common.Models;

namespace AigioL.Common.Storage.Services;

/// <summary>
/// 对象存储服务接口
/// </summary>
public partial interface IObjectStorageService
{
    const string HttpClientName = "对象存储";

    ///// <summary>
    ///// 获取临时下载链接
    ///// </summary>
    ///// <param name="key">对象存储键</param>
    ///// <param name="expiration">有效期</param>
    ///// <returns></returns>
    //string? GetTempDownloadUrl(string key, TimeSpan expiration);

    /// <summary>
    /// 获取 CDN 临时下载链接
    /// </summary>
    /// <param name="resourceAccessPath">资源访问路径</param>
    /// <param name="expiration">有效期</param>
    /// <returns></returns>
    string GetCdnTempDownloadUrl(string resourceAccessPath, TimeSpan expiration);

    /// <summary>
    /// 获取对象存储服务的 Key 的委托
    /// </summary>
    /// <param name="keyPrefix">key 前缀</param>
    /// <param name="fileEx">文件扩展名</param>
    /// <param name="hashHex">哈希值的十六进制表示</param>
    /// <returns>生成的对象存储服务的 Key</returns>
    delegate string GetKeyFuncDelegate(ReadOnlySpan<char> keyPrefix, ReadOnlySpan<char> fileEx, ReadOnlySpan<char> hashHex);

    /// <summary>
    /// 上传文件到对象存储
    /// </summary>
    Task<ApiRsp<Uri?>> UploadAsync(
        string? bucket,
        string keyPrefix,
        Stream stream,
        string hashHex,
        string fileEx,
        GetKeyFuncDelegate? getKeyFunc = null,
        bool leaveOpen = false,
        CancellationToken cancellationToken = default);
}