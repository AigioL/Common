using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 商家扣款协议表实体类
/// </summary>
[Table("MerchantDeductionAgreements")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class MerchantDeductionAgreement :
    Entity<Guid>,
    ICreationTime,
    IUpdateTime,
    INote,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 签约时间
    /// </summary>
    [Comment("签约时间")]
    public DateTimeOffset? SigningTime { get; set; }

    /// <summary>
    /// 解约时间
    /// </summary>
    [Comment("解约时间")]
    public DateTimeOffset? UnSigningTime { get; set; }

    /// <summary>
    /// 平台类型
    /// </summary>
    [Comment("平台类型")]
    public PaymentType Platform { get; set; }

    /// <summary>
    /// 签约协议号
    /// </summary>
    [Required]
    [StringLength(32)]
    [Comment("签约协议号")]
    public required string AgreementNo { get; set; }

    /// <summary>
    /// 用户 OpenId
    /// </summary>
    [StringLength(255)]
    [Comment("用户 OpenId")]
    public string? UserOpenId { get; set; }

    /// <summary>
    /// 用户登录账号
    /// </summary>
    [StringLength(255)]
    [Comment("用户登录账号")]
    public string? UserLoginAccount { get; set; }

    /// <summary>
    /// 外部协议号。支付宝协议号或微信支付委托代扣协议 Id
    /// </summary>
    [StringLength(32)]
    [Comment("外部协议号")]
    public string ExtAgreementNo { get; set; } = "";

    /// <summary>
    /// 生效时间
    /// </summary>
    [Comment("生效时间")]
    public DateTimeOffset? ValidTime { get; set; }

    /// <summary>
    /// 失效时间
    /// </summary>
    [Comment("失效时间")]
    public DateTimeOffset? InvalidTime { get; set; }

    /// <summary>
    /// 周期数
    /// </summary>
    [Comment("周期数")]
    public int Period { get; set; }

    /// <summary>
    /// 周期类型
    /// </summary>
    [Required]
    [StringLength(50)]
    [Comment("周期类型")]
    public required string PeriodType { get; set; }

    /// <summary>
    /// 扣款执行日期
    /// </summary>
    [Comment("扣款执行日期")]
    public DateTimeOffset ExecuteTime { get; set; }

    /// <summary>
    /// 下次扣款时间
    /// </summary>
    [Comment("下次扣款时间")]
    public DateTimeOffset? NextDeductionTime { get; set; }

    /// <summary>
    /// 初次扣款金额
    /// </summary>
    [Precision(18, 2)]
    [Comment("初次扣款金额")]
    public decimal FirstAmount { get; set; }

    /// <summary>
    /// 单次扣款金额
    /// </summary>
    [Precision(18, 2)]
    [Comment("单次扣款金额")]
    public decimal SingleAmount { get; set; }

    /// <summary>
    /// 协议状态
    /// </summary>
    [Comment("协议状态")]
    public AgreementStatus Status { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Comment("更新时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 业务类型
    /// </summary>
    [Comment("业务类型")]
    public int BusinessTypeId { get; set; }

    /// <summary>
    /// 配置 Id
    /// </summary>
    [Comment("配置 Id")]
    public Guid ConfigurationId { get; set; }

    /// <summary>
    /// 通知状态
    /// </summary>
    [Comment("通知状态")]
    public NoticeStatus NoticeStatus { get; set; }

    /// <summary>
    /// 通知次数
    /// </summary>
    [Comment("通知次数")]
    public int NoticeCount { get; set; }

    /// <summary>
    /// 通知完成时间
    /// </summary>
    [Comment("通知完成时间")]
    public DateTimeOffset? NoticeFinishTime { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual List<Order> Orders { get; set; } = null!;

    public virtual MerchantDeductionAgreementConfiguration Configuration { get; set; } = null!;

    //public virtual XunYouMDAExtend XunYouMDAExtend { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<MerchantDeductionAgreement>
    {
        public void Configure(EntityTypeBuilder<MerchantDeductionAgreement> builder)
        {
            builder.HasAlternateKey(a => a.AgreementNo);

            builder.HasOne(o => o.User)
                .WithMany(m => m.MerchantDeductionAgreements)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(o => o.Orders)
                .WithOne(m => m.MerchantDeductionAgreement)
                .HasForeignKey(f => f.MerchantDeductionAgreementId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.Configuration)
                .WithMany(m => m.MerchantDeductionAgreements)
                .HasForeignKey(f => f.ConfigurationId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
