using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;

public sealed partial class StaticResourceTableItemModel : IReadOnlyId<Guid>, ICreateTime, IUpdateTime
{
    public Guid Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// SHA256 哈希值
    /// </summary>
    public string? SHA256 { get; set; }

    /// <summary>
    /// SHA384 哈希值
    /// </summary>
    public string? SHA384 { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件后缀名
    /// </summary>
    public string? FileExtension { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public CloudFileType FileType { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 下载地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 创建人 UserId（创建此条目的后台管理员）
    /// </summary>
    public Guid? CreateUserId { get; set; }

    /// <summary>
    /// 创建人（创建此条目的后台管理员）
    /// </summary>
    public string? CreateUser { get; set; }

    /// <summary>
    /// 最后一次操作的人 UserId（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    /// 最后一次操作的人（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public string? OperatorUser { get; set; }

    /// <summary>
    /// 静态资源上传记录集合
    /// </summary>
    public List<StaticResourceUploadRecordItemModel>? StaticResourceUploadRecords { get; set; }
}

public sealed partial class AddOrEditStaticResourceModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// SHA256 哈希值
    /// </summary>
    public string? SHA256 { get; set; }

    /// <summary>
    /// SHA384 哈希值
    /// </summary>
    public string? SHA384 { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// 文件后缀名
    /// </summary>
    public string? FileExtension { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public CloudFileType FileType { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 下载地址
    /// </summary>
    public string? Url { get; set; }
}