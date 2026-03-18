using AigioL.Common.Primitives.Columns;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// 合作伙伴后台的用户与角色关联表实体
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class PCUserRole :
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

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<PCUserRole>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<PCUserRole> builder)
        {
            builder.HasKey(x => new { x.UserId, x.RoleId, x.TenantId });
        }
    }
}

partial class PCUserRole : ITenantId
{
    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }
}