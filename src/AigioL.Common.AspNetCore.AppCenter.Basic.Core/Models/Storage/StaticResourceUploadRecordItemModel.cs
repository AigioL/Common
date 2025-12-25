using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;

public sealed partial class StaticResourceUploadRecordItemModel : INote, ICreateTime
{
    /// <summary>
    /// 客户端用户
    /// </summary>
    public string? User { get; set; }

    public Guid? UserId { get; set; }

    /// <summary>
    /// 后台用户
    /// </summary>
    public string? BMUser { get; set; }

    public Guid? BMUserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Note { get; set; }
}
