using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem.Abstractions;

/// <summary>
/// 云端存储文件信息
/// </summary>
public interface ICloudFileInfo
{
    /// <summary>
    /// 文件名，为 <see langword="null"/> 时使用 Id.ToStringN() + FileExtension
    /// </summary>
    string? FileName { get; set; }

    /// <summary>
    /// 文件指纹 SHA384
    /// </summary>
    string? SHA384 { get; set; }

    /// <summary>
    /// 文件路径，如果上传在当前服务器上则有一个相对路径，如果存储在第三方云计算提供的存储服务上则为 <see langword="null"/>
    /// </summary>
    string? FilePath { get; set; }

    /// <summary>
    /// 文件后缀名，例如 .exe
    /// </summary>
    string? FileExtension { get; set; }

    /// <summary>
    /// 文件类型，可使用扩展函数 <see cref="CloudFileTypeEnumExtensions.GetFileFormat(ReadOnlySpan{char})"/> 获取图片类型
    /// </summary>
    CloudFileType FileType { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    long FileSize { get; set; }
}