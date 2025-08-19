using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 用户钱包
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserWallet :
    IEntity<Guid>,
    IReadOnlyUserWalletValue,
    IUpdateTime,
    IRowVersion
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Comment("用户 Id")]
    public Guid Id { get; set; }

    public virtual User User { get; set; } = null!;

    /// <inheritdoc/>
    [Precision(18, 4)]
    [Comment("账号余额")]
    public decimal AccountBalance { get; set; }

    /// <inheritdoc/>
    [Precision(18, 4)]
    [Comment("可提现金额")]
    public decimal WithdrawableAmount { get; set; }

    /// <inheritdoc/>
    [Precision(18, 4)]
    [Comment("可用金额")]
    public decimal AvailableAmount { get; set; }

    /// <summary>
    /// 累计充值金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("累计充值金额")]
    public decimal CumulativeRechargeAmount { get; set; }

    /// <summary>
    /// 累计消费金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("累计消费金额")]
    public decimal CumulativeConsumptionAmount { get; set; }

    /// <summary>
    /// 累计奖励金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("累计奖励金额")]
    public decimal CumulativeRewardAmount { get; set; }

    /// <summary>
    /// 累计收益金额
    /// </summary>
    [Precision(18, 4)]
    [Comment("累计收益金额")]
    public decimal CumulativeIncomeAmount { get; set; }

    /// <inheritdoc/>
    [Comment("付费积分")]
    public long ProPoints { get; set; }

    /// <inheritdoc/>
    [Comment("可用付费积分")]
    public long AvailableProPoints { get; set; }

    /// <inheritdoc/>
    [Comment("免费积分")]
    public long FreePoints { get; set; }

    [Comment("修改时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }

    public virtual List<UserWalletChangeRecord> UserWalletChangeRecord { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserWallet>
    {
        public void Configure(EntityTypeBuilder<UserWallet> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserWallets);

            builder.HasOne(u => u.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<UserWallet>(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserWalletChangeRecord)
                   .WithOne()
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// 计算钱包值
    /// </summary>
    /// <param name="valueType">钱包值类型</param>
    /// <param name="value">变动值</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void CalculateWalletValue(UserWalletValueType valueType, decimal value)
    {
        switch (valueType)
        {
            case UserWalletValueType.AccountBalance:
            case UserWalletValueType.AvailableAmount:
                AccountBalance += value;
                AvailableAmount += value;
                // "可提现金额"永远不会大于"账号余额"
                // 当"账号余额"减少到低于"可提现金额"时，"可提现金额"也会同步减少，这是尽量保留"可提现金额"消费策略吧，因为它还可以用于提现。
                WithdrawableAmount = Math.Min(WithdrawableAmount, AccountBalance);
                break;

            case UserWalletValueType.WithdrawableAmount:
                WithdrawableAmount += value;
                goto case UserWalletValueType.AccountBalance; // 同步变更"账号余额"

            case UserWalletValueType.FreePoints:
                FreePoints += (long)value;
                break;

            case UserWalletValueType.ProPoints:
            case UserWalletValueType.AvailableProPoints:
                ProPoints += (long)value;
                AvailableProPoints += (long)value;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(valueType), valueType, null);
        }
    }

    /// <summary>
    /// 转换钱包到钱包值的状态
    /// </summary>
    /// <param name="wallet"></param>
    public static implicit operator UserWalletState(UserWallet? wallet) => wallet == null ? default : new()
    {
        AccountBalance = wallet.AccountBalance,
        WithdrawableAmount = wallet.WithdrawableAmount,
        AvailableAmount = wallet.AvailableAmount,
        ProPoints = wallet.ProPoints,
        AvailableProPoints = wallet.AvailableProPoints,
        FreePoints = wallet.FreePoints,
    };
}
