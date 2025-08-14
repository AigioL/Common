using AigioL.Common.Primitives.Columns;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <summary>
/// 管理后台的用户与角色关联表实体
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ACUserRole :
    IdentityUserRole<Guid>
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public override Guid UserId { get; set; }

    /// <summary>
    /// 角色 Id
    /// </summary>
    [Comment("角色 Id")]
    public override Guid RoleId { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<ACUserRole>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<ACUserRole> builder)
        {
            builder.HasKey(x => new { x.UserId, x.RoleId, x.TenantId });
        }
    }
}

partial class ACUserRole : ITenantId
{
    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }
}