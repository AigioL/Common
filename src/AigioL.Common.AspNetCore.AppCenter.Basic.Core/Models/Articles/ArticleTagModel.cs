using AigioL.Common.AspNetCore.AdminCenter.Columns;
using AigioL.Common.Primitives.Columns;
using System.ComponentModel.DataAnnotations;
namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章标签模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public partial class ArticleTagModel
{
    /// <summary>
    /// 文章 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// 文章标签名
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(LastMKeyIndex)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 最后一个 MessagePack 序列化 下标，继承自此类，新增需要序列化的字段/属性，标记此值+1，+2
    /// </summary>
    protected const int LastMKeyIndex = 1;
}

public sealed partial class AddOrEditArticleTagModel
{
    /// <summary>
    /// 标签名
    /// </summary>
    [StringLength(MaxLengths.ArticleTagName)]
    public string Name { get; set; } = "";
}

public sealed partial class ArticleTagTableItemModel : IReadOnlyId<Guid>, ICreateTime, IUpdateTime, ICreateUserIdNullable, IOperatorUserId
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 标签名
    /// </summary>
    [StringLength(MaxLengths.ArticleTagName)]
    public string Name { get; set; } = "";

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
    /// 文章数量
    /// </summary>
    public int ArticleCount { get; set; }
}

public sealed partial class ArticleTagOptionItemModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 标签名
    /// </summary>
    [StringLength(MaxLengths.ArticleTagName)]
    public string Name { get; set; } = "";
}