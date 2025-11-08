using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 验证码（短信/邮箱验证码）记录表
/// </summary>
[Table(nameof(AuthMessageRecord) + "s")]
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class AuthMessageRecord :
    Entity<Guid>,
    INEWSEQUENTIALID,
    IPhoneNumber,
    IReadOnlyIPAddress,
    ICreationTime
{
    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <inheritdoc/>
    [Comment("手机号")]
    public string? PhoneNumber { get; set; }

    /// <inheritdoc/>
    [Comment("手机号国家或地区代码")]
    public string? PhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [Comment("邮箱")]
    [StringLength(MaxLengths.Email)]
    public string? Email { get; set; }

    /// <inheritdoc/>
    [Required]
    [Comment("IP 地址")]
    [StringLength(MaxLengths.IPAddress)]
    public required string IPAddress { get; set; }

    #region 提供商内容

    /// <summary>
    /// 第三方下发渠道的显示名称
    /// </summary>
    [Required]
    [Comment("第三方下发渠道的显示名称")]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// 第三方提供商返回的内容
    /// </summary>
    [Comment("第三方提供商返回的内容")]
    public string? SendResultRecord { get; set; }

    /// <summary>
    /// 第三方提供商返回的 HTTP 状态码
    /// </summary>
    [Comment("第三方提供商返回的 HTTP 状态码")]
    public int HttpStatusCode { get; set; }

    /// <summary>
    /// 第三方提供商发送是否成功
    /// </summary>
    [Comment("第三方提供商发送是否成功")]
    public bool SendIsSuccess { get; set; }

    #endregion

    /// <summary>
    /// 内容（短信验证码或邮箱验证码的内容）
    /// </summary>
    [Required]
    [Comment("内容")]
    public required string Content { get; set; }

    /// <summary>
    /// 是否校验过
    /// </summary>
    [Comment("是否校验过")]
    public bool EverCheck { get; set; }

    /// <summary>
    /// 是否校验成功
    /// </summary>
    [Comment("是否校验成功")]
    public bool CheckSuccess { get; set; }

    /// <summary>
    /// 是否被废弃
    /// </summary>
    [Comment("是否被废弃")]
    public bool Abandoned { get; set; }

    /// <summary>
    /// 校验失败次数
    /// </summary>
    [Comment("校验失败次数")]
    public int CheckFailuresCount { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid? UserId { get; set; }

    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 类型（是属于邮箱验证还是短信验证）
    /// </summary>
    [Comment("类型")]
    public AuthMessageType Type { get; set; }

    /// <summary>
    /// 发送验证码用途
    /// </summary>
    [Comment("发送验证码用途")]
    public SmsCodeType RequestType { get; set; }

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
