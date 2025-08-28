using System;
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
