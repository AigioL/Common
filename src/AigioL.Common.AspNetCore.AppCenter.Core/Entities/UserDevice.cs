using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 用户设备
/// </summary>
[Table(nameof(UserDevice) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserDevice :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreationTime
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 设备名称
    /// </summary>
    [Comment("设备名称")]
    [StringLength(MaxLengths.CUserDeviceName)]
    public string? DeviceName { get; set; }

    /// <summary>
    /// 设备唯一识别码
    /// </summary>
    [Comment("设备唯一识别码")]
    [StringLength(MaxLengths.DeviceId)]
    public string? DeviceId { get; set; }

    /// <summary>
    /// 网卡 MAC 地址哈希值
    /// </summary>
    [Comment("网卡 MAC 地址哈希值")]
    [StringLength(MaxLengths.SHA384)]
    public string? MacAddressHash { get; set; }

    /// <summary>
    /// 上次登录时间
    /// </summary>
    [Comment("上次登录时间")]
    public DateTimeOffset LastLoginTime { get; set; }

    /// <summary>
    /// 是否信任
    /// </summary>
    [Comment("是否信任")]
    public bool IsTrust { get; set; } = true;

    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 设备所属终端
    /// </summary>
    [Comment("设备所属终端")]
    public DevicePlatform2 Platform { get; set; }

    public virtual List<UserJsonWebToken> JsonWebTokens { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserDevice>
    {
        public void Configure(EntityTypeBuilder<UserDevice> builder)
        {
            builder.HasMany(x => x.JsonWebTokens)
                .WithOne(x => x.UserDevice)
                .HasForeignKey(x => x.UserDeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
