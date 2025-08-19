using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 验证码（短信/邮箱验证码）记录表
/// </summary>
[Table(nameof(AuthMessageRecord) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AuthMessageRecord
{
    public virtual User User { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<AuthMessageRecord>
    {
        public void Configure(EntityTypeBuilder<AuthMessageRecord> builder)
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.AuthMessageRecords)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
