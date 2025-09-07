using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 键值对表（值为字符串）
/// <para>https://stackoverflow.com/questions/514603/key-value-pairs-in-a-database-table</para>
/// </summary>
[Table("KeyValuePairs")]
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public class KeyValuePair :
    IEntity<string>,
    ISoftDeleted
{
    /// <summary>
    /// 键
    /// </summary>
    [Key] // EF 主键
    [Comment("键")]
    public required string Id { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    [Required] // EF not null
    [Comment("值")]
    public required string Value { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }

    string DebuggerDisplay() => $"{Id}, {Value}";
}