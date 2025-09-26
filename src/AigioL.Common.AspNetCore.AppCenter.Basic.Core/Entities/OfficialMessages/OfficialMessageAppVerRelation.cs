using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;

/// <summary>
/// 官方消息与客户端版本关联实体类
/// </summary>
[PrimaryKey(nameof(OfficialMessageId), nameof(AppVerId))]
[Table(nameof(OfficialMessageAppVerRelation) + "s")]
public partial class OfficialMessageAppVerRelation
{
    /// <summary>
    /// 官方消息 Id
    /// </summary>
    [Comment("官方消息 Id")]
    public Guid OfficialMessageId { get; set; }

    /// <summary>
    /// 客户端版本 Id
    /// </summary>
    [Comment("客户端版本 Id")]
    public Guid AppVerId { get; set; }
}
