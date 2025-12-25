using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice;

public sealed partial class OfficialMessageTableItemModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public OfficialMessageType MessageType { get; set; }

    [StringLength(MaxLengths.LongTitle)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [StringLength(MaxLengths.Text)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息链接
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string? MessageLink { get; set; }

    /// <summary>
    /// 推送设备
    /// </summary>
    public ClientPlatform PushClientDevice { get; set; }

    /// <summary>
    /// 推送时间
    /// </summary>
    public DateTimeOffset PushTime { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset? ExpireTime { get; set; }

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
    /// 用户可见
    /// </summary>
    public bool UserViewable { get; set; }

    /// <summary>
    /// 推送客户端版本
    /// </summary>
    public List<Guid> PushAppVersions { get; set; } = [];
}

public sealed partial class AddOrEditOfficialMessageModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public OfficialMessageType MessageType { get; set; }

    [StringLength(MaxLengths.LongTitle)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [StringLength(MaxLengths.Text)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息链接
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string? MessageLink { get; set; }

    /// <summary>
    /// 推送设备
    /// </summary>
    public ClientPlatform PushClientDevice { get; set; }

    /// <summary>
    /// 推送时间
    /// </summary>
    public DateTimeOffset PushTime { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset? ExpireTime { get; set; }

    /// <summary>
    /// 推送客户端版本
    /// </summary>
    public List<Guid> PushAppVersions { get; set; } = [];
}