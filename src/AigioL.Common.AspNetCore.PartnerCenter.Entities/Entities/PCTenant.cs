using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// 合作伙伴后台的租户表实体
/// </summary>
[Table("PCTenants")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCTenant :
    OperatorBaseEntity<Guid>,
    INEWSEQUENTIALID,
    IDisable,
    INote,
    ISoftDeleted,
    IRowVersion
{
    /// <summary>
    /// 租户名称
    /// </summary>
    [Required]
    [StringLength(MaxLengths.NickName)]
    [Comment("租户名称")]
    public required string Name { get; set; }

    /// <summary>
    /// 租户唯一编码
    /// </summary>
    [Comment("租户唯一编码")]
    public string? UniqueCode { get; set; }

    /// <summary>
    /// 联系人
    /// </summary>
    [StringLength(MaxLengths.NickName)]
    [Comment("联系人")]
    public string? Contact { get; set; }

    /// <summary>
    /// 联系人电话
    /// </summary>
    [Comment("联系人电话")]
    [StringLength(IPhoneNumber.DatabaseMaxLength)]
    public string? ContactPhoneNumber { get; set; }

    /// <summary>
    /// 联系人电话国家或地区代码
    /// </summary>
    [Comment("联系人电话国家或地区代码")]
    [StringLength(IPhoneNumber.RegionCodeDatabaseMaxLength)]
    public string? ContactPhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 注册人电话
    /// </summary>
    [Comment("注册人电话")]
    [StringLength(IPhoneNumber.DatabaseMaxLength)]
    public string? RegisterPhoneNumber { get; set; }

    /// <summary>
    /// 注册人电话国家或地区代码
    /// </summary>
    [Comment("注册人电话国家或地区代码")]
    [StringLength(IPhoneNumber.RegionCodeDatabaseMaxLength)]
    public string? RegisterPhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    [StringLength(MaxLengths.RealityAddress)]
    [Comment("地址")]
    public string? Address { get; set; }

    /// <summary>
    /// 注册邮箱
    /// </summary>
    [StringLength(MaxLengths.Email)]
    [Comment("注册邮箱")]
    public string? RegisterEmail { get; set; }

    /// <summary>
    /// 审核人 Id
    /// </summary>
    [Comment("审核人 Id")]
    public Guid? AuditorId { get; set; }

    /// <summary>
    /// 审核人
    /// </summary>
    public BMUser? Auditor { get; set; }

    /// <summary>
    /// 审核时间
    /// </summary>
    [Comment("审核时间")]
    public DateTimeOffset? AuditTime { get; set; }

    /// <summary>
    /// 审核状态
    /// </summary>
    [Comment("审核状态")]
    public BMTenantAuditStatus AuditStatus { get; set; }

    /// <summary>
    /// 审核备注
    /// </summary>
    [Comment("审核备注")]
    public string? AuditNote { get; set; }

    /// <summary>
    /// 授权开始时间
    /// </summary>
    [Comment("授权开始时间")]
    public DateTimeOffset? AuthorizationStartTime { get; set; }

    /// <summary>
    /// 授权结束时间
    /// </summary>
    [Comment("授权结束时间")]
    public DateTimeOffset? AuthorizationEndTime { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 是否为平台管理员
    /// </summary>
    [Comment("是否为平台管理员")]
    public bool IsPlatformAdministrator { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public DateTimeOffset? DeleteTime { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }

    /// <summary>
    /// 管理后台网站名称
    /// </summary>
    [Comment("网站名称")]
    [StringLength(MaxLengths.LongName)]
    public string? WebsiteName { get; set; }

    /// <summary>
    /// 管理后台网站域名
    /// </summary>
    [Comment("网站域名")]
    [StringLength(MaxLengths.Url)]
    public string? WebsiteDomainName { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<PCTenant>
    {
        /// <inheritdoc/>
        public sealed override void Configure(EntityTypeBuilder<PCTenant> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.Auditor)
                .WithMany(x => x.AuditorPCTenants)
                .HasForeignKey(x => x.AuditorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
