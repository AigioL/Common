using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.PartnerCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// PC 用户提现记录
/// </summary>
[Table("PCUserWithdrawalRecords")]
[Index(nameof(UserId))]
[Index(nameof(WithdrawalNumber), IsUnique = true)]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCUserWithdrawalRecord :
    Entity<Guid>,
    INEWSEQUENTIALID,
    INote,
    ICreateTime
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 提现单号
    /// </summary>
    [Required]
    [StringLength(64)]
    [Comment("提现单号")]
    public required string WithdrawalNumber { get; set; }

    /// <summary>
    /// 提现金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("提现金额")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 提现状态
    /// </summary>
    [Comment("提现状态")]
    public PCUserWithdrawalStatus Status { get; set; }

    /// <summary>
    /// 支付平台
    /// </summary>
    [Comment("支付平台")]
    public PaymentType? PaymentPlatform { get; set; }

    /// <summary>
    /// 第三方平台转账订单号
    /// </summary>
    [StringLength(64)]
    [Comment("第三方平台转账订单号")]
    public string? ThirdPartyPlatformNumber { get; set; }

    /// <summary>
    /// 微信 OpenId
    /// </summary>
    [StringLength(255)]
    [Comment("微信 OpenId")]
    public string? UserOpenId { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    [Comment("失败原因")]
    public string? FailureReason { get; set; }

    /// <summary>
    /// 申请时间
    /// </summary>
    [Comment("申请时间")]
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [Comment("完成时间")]
    public DateTimeOffset? FinishTime { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    public virtual PCUserWallet Wallet { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PCUserWithdrawalRecord>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PCUserWithdrawalRecord> builder)
        {
            builder.HasOne(u => u.Wallet)
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
