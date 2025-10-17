using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;

/// <summary>
/// 会员充值业务订单表实体类
/// </summary>
[Table(nameof(MembershipBusinessOrder) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class MembershipBusinessOrder :
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
    /// 会员商品充值分类 Id
    /// </summary>
    [Comment("会员商品 Id")]
    public Guid MembershipGoodsId { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    [Comment("商品名称")]
    public required string GoodsName { get; set; }

    /// <summary>
    /// 商品编号
    /// </summary>
    [Comment("商品编号")]
    public required string GoodsNo { get; set; }

    /// <summary>
    /// 会员订阅类型，跟随变更记录更新
    /// </summary>
    [Comment("会员订阅类型")]
    public MembershipLicenseFlags MemberLicenseType { get; set; }

    /// <summary>
    /// 充值天数
    /// </summary>
    [Comment("充值天数")]
    public int RechargeDays { get; set; }

    /// <summary>
    /// 订单 Id
    /// </summary>
    [Required]
    [StringLength(MaxLengths.OrderId)]
    [Comment("订单 Id")]
    public required string OrderId { get; set; }

    /// <summary>
    /// 应收金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("应收金额")]
    public decimal AmountReceivable { get; set; }

    /// <summary>
    /// 实收金额
    /// </summary>
    [Required]
    [Precision(18, 4)]
    [Comment("实收金额")]
    public decimal AmountReceived { get; set; }

    /// <summary>
    /// 支付状态
    /// </summary>
    [Comment("支付状态")]
    public OrderStatus PaymentStatus { get; set; }

    /// <summary>
    /// 商品充值状态
    /// </summary>
    [Comment("商品充值状态")]
    public GoodRechargeStatus GoodsRechargeStatus { get; set; }

    /// <summary>
    /// 充值完成时间
    /// </summary>
    [Comment("充值完成时间")]
    public DateTimeOffset? RechargeCompletionTime { get; set; }

    /// <summary>
    /// 支付时间
    /// </summary>
    [Comment("支付时间")]
    public DateTimeOffset? PaymentTime { get; set; }

    /// <summary>
    /// 业务来源(普通订单,Key激活)
    /// </summary>
    [Required]
    [Comment("业务来源")]
    public MembershipBusinessSource BusinessSource { get; set; }

    /// <summary>
    /// Key 激活记录 Id
    /// </summary>
    [Comment("Key 激活记录 Id")]
    public Guid? ProductKeyRecordId { get; set; }

    /// <summary>
    /// 商家扣款协议 Id
    /// </summary>
    [Comment("商家扣款协议 Id")]
    public Guid? MerchantDeductionAgreementId { get; set; }

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <inheritdoc/>
    [Comment("更新时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    public virtual MembershipProductKeyRecord ProductKeyRecord { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual MerchantDeductionAgreement MerchantDeductionAgreement { get; set; } = null!;

    public virtual MembershipGoods MembershipGoods { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<MembershipBusinessOrder>
    {
        public void Configure(EntityTypeBuilder<MembershipBusinessOrder> builder)
        {
            builder.HasOne(o => o.User)
                .WithMany(m => m.MembershipBusinessOrders)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.Order)
                .WithOne()
                .HasForeignKey<MembershipBusinessOrder>(f => f.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.MembershipGoods)
                .WithMany(m => m.MembershipBusinessOrders)
                .HasForeignKey(f => f.MembershipGoodsId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.ProductKeyRecord)
                  .WithOne()
                  .HasForeignKey<MembershipBusinessOrder>(f => f.ProductKeyRecordId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(o => o.MerchantDeductionAgreement)
                .WithMany()
                .HasForeignKey(o => o.MerchantDeductionAgreementId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}