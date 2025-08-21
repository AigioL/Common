using AigioL.Common.AspNetCore.AdminCenter.Entities.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;

/// <summary>
/// 静态资源上传记录表实体类
/// </summary>
[Table("StaticResourceUploadRecords")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public class StaticResourceUploadRecord :
    TenantBaseEntity<Guid>,
    INEWSEQUENTIALID,
    INote
{
    /// <summary>
    /// 上传的用户 Id
    /// </summary>
    [Comment("上传的用户 Id")]
    public Guid? UserId { get; set; }

    /// <summary>
    /// 上传的用户
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 静态资源 Id
    /// </summary>
    [Comment("静态资源 Id")]
    public Guid StaticResourceId { get; set; }

    public virtual StaticResource StaticResource { get; set; } = null!;

    /// <inheritdoc/>
    [Comment("备注")]
    public string? Note { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<StaticResourceUploadRecord>
    {
        public void Configure(EntityTypeBuilder<StaticResourceUploadRecord> builder)
        {
            // 上传记录关联客户端侧用户
            builder
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // 上传记录与静态资源关联
            builder
                .HasOne(a => a.StaticResource)
                .WithMany(a => a.StaticResourceUploadRecords)
                .HasForeignKey(a => a.StaticResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}