using AigioL.Common.AspNetCore.AppCenter.Helpers.Uploads;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;

/// <summary>
/// 静态资源上传结果
/// </summary>
public partial class StaticResourceUploadResult<TCode>
    where TCode : struct
{
    /// <summary>
    /// 文件名 前端匹配上传对应的 Item
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 上传成功后 Url 地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 文件指纹 SHA384
    /// </summary>
    public string? SHA384 { get; set; }

    /// <summary>
    /// 文件上传结果状态码
    /// </summary>
    public TCode Code { get; set; }
}

/// <inheritdoc cref="StaticResourceUploadResult{TCode}"/>
public sealed partial class StaticResourceUploadResult : StaticResourceUploadResult<UploadFileCode>
{
    public static implicit operator StaticResourceUploadResult(UploadFileInfo info)
    {
        return new StaticResourceUploadResult
        {
            FileName = info.FileName ?? string.Empty,
            Url = info.Url?.ToString(),
            SHA384 = info.SHA384,
            Code = UploadFileCode.Ok
        };
    }
}