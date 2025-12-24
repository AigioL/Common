using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;

/// <summary>
/// 广告表格项模型
/// </summary>
public sealed partial record class KomaasharuTableItem : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 备注
    /// </summary>
    public string? Describe { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 跳转地址
    /// </summary>
    public string JumpUrl { get; set; } = string.Empty;

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// 广告类型
    /// </summary>
    public KomaasharuType Type { get; set; } = KomaasharuType.Banner;

    /// <summary>
    /// 总点击数
    /// </summary>
    public long TotalClick { get; set; }

    /// <summary>
    /// 总展示数
    /// </summary>
    public long TotalDisplay { get; set; }

    /// <summary>
    /// 广告方向
    /// </summary>
    public KomaasharuOrientation Orientation { get; set; } = KomaasharuOrientation.Horizontal;

    /// <summary>
    /// 排序
    /// </summary>
    public long Order { get; set; }

    /// <summary>
    /// 推送的平台
    /// </summary>
#pragma warning disable CS0618 // 类型或成员已过时
    public WebApiCompatDevicePlatform Platform { get; set; }
#pragma warning restore CS0618 // 类型或成员已过时

    /// <summary>
    /// 推送的设备
    /// </summary>
    public DeviceIdiom DeviceIdiom { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disable { get; set; }

    /// <summary>
    /// 是否过期
    /// </summary>
    public bool Expired { get; set; }
}
