using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.Primitives.Columns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 渠道包信息实体类
/// </summary>
[Table("ChannelPackages")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ChannelPackage :
    OperatorBaseEntity<Guid>,
    INote
{
    /// <summary>
    /// 渠道名称
    /// </summary>
    [Comment("渠道名称")]
    [StringLength(MaxLengths.Name)]
    public required string Name { get; set; }

    /// <inheritdoc/>
    [Comment("备注")]
    [StringLength(MaxLengths.Text)]
    public string? Note { get; set; }

    public sealed class EntityTypeConfiguration : EntityTypeConfiguration<ChannelPackage>
    {
        public sealed override void Configure(EntityTypeBuilder<ChannelPackage> builder)
        {
            base.Configure(builder);
        }
    }
}
