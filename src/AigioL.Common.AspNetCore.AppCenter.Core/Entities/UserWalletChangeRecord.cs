using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 用户钱包变更记录表实体类
/// </summary>
[Index(nameof(UserId))]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserWalletChangeRecord :
    Entity<Guid>,
    INEWSEQUENTIALID,
    INote,
    ICreationTime
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 值类型
    /// </summary>
    [Comment("值类型")]
    public UserWalletValueType Type { get; set; }

    /// <summary>
    /// 事件
    /// </summary>
    [Comment("事件")]
    public UserWalletValueEvent Event { get; set; }

    /// <summary>
    /// 支付方向
    /// </summary>
    [Comment("支付方向")]
    public UserWalletPaymentDirection Direction { get; set; }

    /// <summary>
    /// 变更值
    /// </summary>
    [Precision(18, 4)]
    [Comment("变更值")]
    public decimal ChangeValue { get; set; }

    /// <summary>
    /// 结果值
    /// </summary>
    [Comment("结果值")]
    [Precision(18, 4)]
    public decimal ResultValue { get; set; }

    /// <summary>
    /// 变更原因
    /// </summary>
    [StringLength(MaxLengths.ChangeReason)]
    [Comment("变更原因")]
    public string? Reason { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    /// <summary>
    /// 变更时间
    /// </summary>
    [Comment("变更时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 来源 Id
    /// 用于商城端修改用户钱包时，保证幂等性不会重复修改用户钱包。
    /// 也用于记录数据来源，方便系统之间连接。
    /// 如果只是自己的业务，就使用随机的 Guid 就行。
    /// </summary>
    [Comment("来源 Id")]
    [StringLength(50)]
    public string? SourceId { get; set; }

    /// <summary>
    /// 通知状态
    /// </summary>
    [Comment("通知状态")]
    public bool NoticeStatus { get; set; }

    public virtual User User { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserWalletChangeRecord>
    {
        public void Configure(EntityTypeBuilder<UserWalletChangeRecord> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserWalletChangeRecords);

            builder.HasIndex(x => x.SourceId).IsUnique();
        }
    }
}
