using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;

/// <summary>
/// 会员商品表实体类
/// </summary>
[Table("MembershipGoods")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class MembershipGoods :
    OperatorBaseEntity<Guid>,
    INote,
    ICreationTime,
    IUpdateTime,
    INEWSEQUENTIALID
{
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
    /// 首充原价
    /// </summary>
    [Precision(18, 4)]
    [Comment("首充原价")]
    public decimal? FirstPrice { get; set; }

    /// <summary>
    /// 首充当前价格
    /// </summary>
    [Precision(18, 4)]
    [Comment("首充当前价格")]
    public decimal? FirstCurrentPrice { get; set; }

    /// <summary>
    /// 原价
    /// </summary>
    [Precision(18, 4)]
    [Comment("原价")]
    public decimal Price { get; set; }

    /// <summary>
    /// 当前价格
    /// </summary>
    [Precision(18, 4)]
    [Comment("当前价格")]
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// 是否上架
    /// </summary>
    [Comment("是否上架")]
    public bool Enable { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 商家扣款协议
    /// </summary>
    public virtual List<MerchantDeductionAgreementConfiguration> MerchantDeductionAgreementConfigurations { get; set; } = null!;

    /// <summary>
    /// 会员充值业务订单
    /// </summary>
    public virtual List<MembershipBusinessOrder> MembershipBusinessOrders { get; set; } = null!;

    /// <summary>
    /// 会员商品用户首次购买记录
    /// </summary>
    public virtual List<MembershipGoodsUserFirstRecord> MembershipGoodsUserFirstRecords { get; set; } = null!;

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<MembershipGoods>
    {
        public override void Configure(EntityTypeBuilder<MembershipGoods> builder)
        {
            base.Configure(builder);

            builder.HasMany<MerchantDeductionAgreementConfiguration>(x => x.MerchantDeductionAgreementConfigurations)
                .WithMany()
                .UsingEntity<MembershipGoodsMDARelation>(
                    j => j.HasOne(x => x.MerchantDeductionAgreementConfiguration)
                        .WithMany()
                        .HasForeignKey(x => x.MerchantDeductionAgreementConfigurationId),
                    j => j.HasOne(x => x.MembershipGoods)
                        .WithMany()
                        .HasForeignKey(x => x.MembershipGoodsId),
                    j =>
                        j.HasKey(x => new { x.MembershipGoodsId, x.MerchantDeductionAgreementConfigurationId })
                );

            builder.HasMany<User>()
                .WithMany()
                .UsingEntity<MembershipGoodsUserFirstRecord>(
                    j => j.HasOne(x => x.User)
                        .WithMany()
                        .HasForeignKey(x => x.UserId),
                    j => j.HasOne(x => x.MembershipGoods)
                        .WithMany(x => x.MembershipGoodsUserFirstRecords)
                        .HasForeignKey(x => x.MembershipGoodsId),
                    j =>
                        j.HasKey(x => new { x.MembershipGoodsId, x.UserId })
                );
        }
    }
}