using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
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

[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
[Table("TransferOrders")]
public partial class TransferOrder :
    OperatorBaseEntity<Guid>,
    ITenantId,
    INote,
    INEWSEQUENTIALID
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 租户 Id
    /// </summary>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// 转账单号
    /// </summary>
    [Required]
    [StringLength(64)]
    [Comment("转账单号")]
    public required string TransferNumber { get; set; }

    /// <summary>
    /// 转账标题
    /// </summary>
    [Required]
    [StringLength(128)]
    [Comment("转账标题")]
    public required string Title { get; set; }

    /// <summary>
    /// 转账金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("转账金额")]
    public decimal TransferAmount { get; set; }

    /// <summary>
    /// 支付平台
    /// </summary>
    [Comment("支付平台")]
    public PaymentType PaymentPlatform { get; set; }

    /// <summary>
    /// 用户 OpenId
    /// </summary>
    [Required]
    [StringLength(255)]
    [Comment("用户 OpenId")]
    public required string UserOpenId { get; set; }

    /// <summary>
    /// 用户登录账号
    /// </summary>
    [StringLength(255)]
    [Comment("用户登录账号")]
    public string? UserLoginAccount { get; set; }

    /// <summary>
    /// 用户姓名
    /// </summary>
    [StringLength(64)]
    [Comment("用户姓名")]
    public string? UserRealName { get; set; }

    /// <summary>
    /// 第三方平台转账订单号
    /// </summary>
    [StringLength(64)]
    [Comment("第三方平台转账订单号")]
    public string? ThirdPartyPlatformNumber { get; set; }

    /// <summary>
    /// 转账状态
    /// </summary>
    [Comment("转账状态")]
    public TransferStatus TransferStatus { get; set; }

    /// <summary>
    /// 转账完成时间
    /// </summary>
    [Comment("转账完成时间")]
    public DateTimeOffset? FinishTime { get; set; }

    /// <summary>
    /// 转账失败原因
    /// </summary>
    [Comment("转账失败原因")]
    public string? FailureReason { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    public virtual User User { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<TransferOrder>
    {
        public sealed override void Configure(EntityTypeBuilder<TransferOrder> builder)
        {
            base.Configure(builder);

            builder.HasAlternateKey(o => o.TransferNumber);

            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.OperatorUser)
                .WithMany()
                .HasForeignKey(p => p.OperatorUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}