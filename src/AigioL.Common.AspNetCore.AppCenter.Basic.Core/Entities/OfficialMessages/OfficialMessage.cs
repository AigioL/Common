using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;

/// <summary>
/// 官方消息实体类
/// </summary>
[Index(nameof(CreationTime), IsDescending = new[] { true })]
[Table(nameof(OfficialMessage) + "s")]
public class OfficialMessage :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    ITitle
{
    /// <summary>
    /// 消息类型
    /// </summary>
    [Comment("消息类型")]
    public OfficialMessageType MessageType { get; set; }

    /// <inheritdoc/>
    [StringLength(MaxLengths.LongTitle)]
    [Comment("标题")]
    public string? Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [StringLength(MaxLengths.Text)]
    [Comment("内容")]
    public string? Content { get; set; }

    /// <summary>
    /// 消息链接
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("消息链接")]
    public string? MessageLink { get; set; }

    /// <summary>
    /// 推送设备
    /// </summary>
    [Comment("推送设备")]
    public ClientPlatform PushClientDevice { get; set; }

    ///// <summary>
    ///// 推送渠道（渠道包）
    ///// </summary>
    //[Comment("推送渠道")]
    //public int PushClientChannelId { get; set; }

    /// <summary>
    /// 推送时间
    /// </summary>
    [Comment("推送时间")]
    public DateTimeOffset PushTime { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [Comment("过期时间")]
    public DateTimeOffset? ExpireTime { get; set; }
}
