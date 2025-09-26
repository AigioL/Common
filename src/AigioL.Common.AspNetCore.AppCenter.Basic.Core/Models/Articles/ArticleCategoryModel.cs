namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章分类模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public partial record class ArticleCategoryModel
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