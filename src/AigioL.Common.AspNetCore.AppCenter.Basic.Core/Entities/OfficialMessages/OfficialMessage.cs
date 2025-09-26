using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;

/// <summary>
/// 官方消息实体类
/// </summary>
[Index(nameof(CreationTime), IsDescending = new[] { true })]
[Table(nameof(OfficialMessage) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class OfficialMessage :
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

    /// <summary>
    /// 指定客户端版本关联实体
    /// </summary>
    public virtual List<OfficialMessageAppVerRelation> TargetAppVerRelations { get; set; } = null!;

    /// <summary>
    /// 指定客户端版本
    /// </summary>
    public virtual List<AppVer> TargetAppVers { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<OfficialMessage>
    {
        public override void Configure(EntityTypeBuilder<OfficialMessage> builder)
        {
            base.Configure(builder);

            builder
                .HasMany(u => u.TargetAppVers)
                .WithMany()
                .UsingEntity<OfficialMessageAppVerRelation>(
                    r => r.HasOne<AppVer>()
                        .WithMany()
                        .HasForeignKey(a => a.AppVerId),
                    l => l.HasOne<OfficialMessage>()
                        .WithMany(a => a.TargetAppVerRelations)
                        .HasForeignKey(a => a.OfficialMessageId));
        }
    }
}
