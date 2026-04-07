using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;

public sealed partial record class KomaasharuEdit : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    public string Name { get; set; } = "";

    /// <summary>
    /// 备注
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    [Url]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 跳转地址
    /// </summary>
    [Url]
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
    public KomaasharuType Type { get; set; }

    /// <summary>
    /// 广告方向
    /// </summary>
    public KomaasharuOrientation Orientation { get; set; }

    /// <summary>
    /// 客户端动作
    /// </summary>
    public KomaasharuClientAction ClientAction { get; set; } = KomaasharuClientAction.Display;

    [Obsolete("use Sort")]
    public long Order
    {
        get => Sort;
        set => Sort = value;
    }

    /// <summary>
    /// 排序
    /// </summary>
    public long Sort { get; set; }

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
    /// IsAuth
    /// </summary>
    public bool IsAuth { get; set; }
}
