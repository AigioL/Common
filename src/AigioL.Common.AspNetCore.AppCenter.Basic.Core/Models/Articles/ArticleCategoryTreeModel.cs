namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

/// <summary>
/// 文章分类树状模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public partial record class ArticleCategoryTreeModel : ArticleCategoryModel
{
    /// <summary>
    /// 文章分类子节点
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(LastMKeyIndex + 1)]
    public ArticleCategoryTreeModel[]? Child { get; set; }
}