namespace AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;

/// <summary>
/// 广告模型
/// </summary>
public sealed partial record class KomaasharuModel
{
    /// <summary>
    /// 广告 Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 排序值
    /// </summary>
    public long Sort { get; set; }

    /// <summary>
    /// 广告描述
    /// </summary>
    public string? Desc { get; set; }

    /// <inheritdoc cref="KomaasharuType"/>
    public KomaasharuType Type { get; set; }

    /// <inheritdoc cref="KomaasharuOrientation"/>
    public KomaasharuOrientation Orientation { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }

    public bool IsAuth { get; set; }
}
