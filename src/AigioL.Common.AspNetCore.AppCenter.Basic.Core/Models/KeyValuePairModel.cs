using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models;

public sealed partial class KeyValuePairTableItemModel : IReadOnlyId<string>
{
    /// <summary>
    /// 键
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// 是否软删除
    /// </summary>
    public DateTimeOffset? DeleteTime { get; set; }

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
}

public sealed partial class AddOrEditKeyValuePairModel : IReadOnlyId<string>
{
    /// <summary>
    /// 键
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public required string Value { get; set; }
}
