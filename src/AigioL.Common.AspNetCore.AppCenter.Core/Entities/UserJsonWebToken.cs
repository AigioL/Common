using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// JwtId 到 UserId 关联表
/// <para>JwtId 到 UserId 的映射，允许多设备登录，或可选的互斥实现挤下线</para>
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserJsonWebToken
{
    /// <summary>
    /// Jwt 的用户 Id
    /// </summary>
    [Key]
    [global::MemoryPack.MemoryPackOrder(0)]
    [Comment("JwtUserId")]
    public Guid Id { get; set; }

    /// <summary>
    /// 用户设备 Id
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    [Comment("用户设备 Id")]
    public Guid UserDeviceId { get; set; }

    /// <summary>
    /// 关联的用户设备
    /// </summary>
    [global::MemoryPack.MemoryPackIgnore]
    [IgnoreDataMember]
    [global::System.Text.Json.Serialization.JsonIgnore]
    public virtual UserDevice UserDevice { get; set; } = null!;

    /// <summary>
    /// 关联的 JWT 刷新数据
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public virtual UserRefreshJsonWebToken Refresh { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserJsonWebToken>
    {
        public void Configure(EntityTypeBuilder<UserJsonWebToken> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserJsonWebTokens);

            builder.HasOne(x => x.Refresh)
                .WithOne(x => x.JWT)
                .HasForeignKey<UserRefreshJsonWebToken>(x => x.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
