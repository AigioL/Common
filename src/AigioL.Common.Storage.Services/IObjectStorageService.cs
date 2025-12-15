namespace AigioL.Common.Storage.Services;

/// <summary>
/// 对象存储服务接口
/// </summary>
public partial interface IObjectStorageService
{
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
}
