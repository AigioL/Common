using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// PC 用户配置
/// </summary>
[Table("PCUserConfigs")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCUserConfig :
    OperatorBaseEntity<Guid>
{
    /// <summary>
    /// 用户 Id (与 PCUser 一一对应)
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Comment("用户 Id")]
    public override Guid Id { get; set; }

    /// <summary>
    /// 关联的 PC 用户
    /// </summary>
    public virtual PCUser PCUser { get; set; } = null!;

    /// <summary>
    /// 绑定的微信 OpenId
    /// </summary>
    [Comment("绑定的微信 OpenId")]
    [StringLength(MaxLengths.WeChatId)]
    public string? OpenId { get; set; }

    /// <summary>
    /// 真实姓名 金额大于 2000 时需要
    /// </summary>
    [Comment("真实姓名")]
    [StringLength(MaxLengths.CNCitizenName)]
    public string? RealName { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PCUserConfig>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PCUserConfig> builder)
        {
            builder.HasOne(u => u.PCUser)
                .WithOne()
                .HasForeignKey<PCUserConfig>(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
