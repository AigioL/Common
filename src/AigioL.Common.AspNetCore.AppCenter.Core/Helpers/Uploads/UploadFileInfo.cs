namespace AigioL.Common.AspNetCore.AppCenter.Helpers.Uploads;

public sealed record class UploadFileInfo
{
    /// <summary>
    /// 访问地址
    /// </summary>
    public Uri? Url { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// SHA384 值
    /// </summary>
    public string? SHA384 { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string? FileEx { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }
}