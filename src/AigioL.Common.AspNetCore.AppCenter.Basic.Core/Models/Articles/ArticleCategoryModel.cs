using AigioL.Common.AspNetCore.AdminCenter.Columns;
using AigioL.Common.Primitives.Columns;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章分类模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public partial record class ArticleCategoryModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 文章 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// 分类名
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(LastMKeyIndex)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父级 Id
    /// </summary>
    [global::System.Text.Json.Serialization.JsonIgnore]
    [global::MemoryPack.MemoryPackIgnore]
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 最后一个 MessagePack 序列化 下标，继承自此类，新增需要序列化的字段/属性，标记此值+1，+2
    /// </summary>
    protected const int LastMKeyIndex = 1;
}

public sealed partial class AddOrEditArticleCategoryModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 父级 Id
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 分类名
    /// </summary>
    [StringLength(maximumLength: MaxLengths.ArticleCategoryName, MinimumLength = 1, ErrorMessage = $"{{0}} 最多 {MaxLengths.ArticleCategoryNameString} 个字符")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 排序
    /// </summary>
    public long Sort { get; set; }

    [Obsolete("use Sort")]
    public long Order { get => Sort; set => Sort = value; }
}

public sealed partial class ArticleCategoryTableItemModel : IReadOnlyId<Guid>, ISort, ICreateTime, IUpdateTime, ICreateUserIdNullable, IOperatorUserId
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 父级 Id
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 分类名
    /// </summary>
    [StringLength(maximumLength: MaxLengths.ArticleCategoryName, MinimumLength = 1, ErrorMessage = $"{{0}} 最多 {MaxLengths.ArticleCategoryNameString} 个字符")]
    public string Name { get; set; } = "";

    /// <summary>
    /// 排序
    /// </summary>
    public long Sort { get; set; }

    [Obsolete("use Sort")]
    public long Order { get => Sort; set => Sort = value; }

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

/// <summary>
/// 文章分类树节点
/// </summary>
public sealed partial class ArticleCategoryTreeNodeModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    [StringLength(maximumLength: MaxLengths.ArticleCategoryName, MinimumLength = 1, ErrorMessage = $"{{0}} 最多 {MaxLengths.ArticleCategoryNameString} 个字符")]
    public string Name { get; set; } = "";

    public ArticleCategoryTreeNodeModel[] Children { get; set; } = [];
}