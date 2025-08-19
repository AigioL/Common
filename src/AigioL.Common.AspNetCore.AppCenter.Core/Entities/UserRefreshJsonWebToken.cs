using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// JWT 刷新 Token 纪录表
/// <para>JwtId 的 RefreshToken 保存表</para>
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public partial class UserRefreshJsonWebToken
    : IEntity<Guid>
{
    /// <summary>
    /// Jwt 的用户 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Comment("JwtUserId")]
    public Guid Id { get; set; }

    /// <summary>
    /// JWT 刷新 Token 值
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    [Required]
    [Comment("JWT 刷新 Token 值")]
    public required string RefreshToken { get; set; }

    /// <summary>
    /// JWT 刷新 Token 值有效期
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    [Comment("JWT 刷新 Token 值有效期")]
    public DateTimeOffset RefreshExpiration { get; set; }

    /// <summary>
    /// JWT 禁止在此时间之前刷新
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    [Comment("JWT 禁止在此时间之前刷新")]
    public DateTimeOffset NotBefore { get; set; }

    /// <summary>
    /// JWT 值生成算法版本
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    [Comment("JWT 值生成算法版本")]
    public byte Version { get; set; } = 1;

    /// <summary>
    /// 关联的 JWT 数据
    /// </summary>
    [global::MemoryPack.MemoryPackIgnore]
    [IgnoreDataMember]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public virtual UserJsonWebToken? JWT { get; set; }

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserRefreshJsonWebToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshJsonWebToken> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserRefreshJsonWebTokens);
        }
    }
}
