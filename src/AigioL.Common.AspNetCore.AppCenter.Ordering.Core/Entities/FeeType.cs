using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;

/// <summary>
/// 费用类型表实体类
/// </summary>
[Table("FeeTypes")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class FeeType :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    INote,
    IDisable
{
    /// <summary>
    /// 费用类型名称
    /// </summary>
    [Required]
    [StringLength(MaxLengths.CouponName)]
    [Comment("费用类型名称")]
    public required string Name { get; set; }

    [Comment("是否无效")]
    public bool Disable { get; set; }

    /// <summary>
    /// 费用方向，<see langword="false"/> 表示进，<see langword="true"/> 表示出
    /// </summary>
    [Comment("费用方向")]
    public bool Direction { get; set; }

    [Comment("备注")]
    public string? Note { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<FeeType>
    {
        public sealed override void Configure(EntityTypeBuilder<FeeType> builder)
        {
            base.Configure(builder);
        }
    }
}