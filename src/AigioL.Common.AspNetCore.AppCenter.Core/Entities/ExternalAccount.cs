using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using AigioL.Common.Primitives.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 外部账号
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class ExternalAccount :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ICreationTime,
    INickName,
    IUpdateTime
{
    /// <summary>
    /// 用户 Id
    /// </summary>
    [Comment("用户 Id")]
    public Guid? UserId { get; set; }

    /// <summary>
    /// 第三方账号 Id
    /// </summary>
    [Required]
    [Comment("第三方账号 Id")]
    public required string ExternalAccountId { get; set; }

    /// <summary>
    /// 第三方账号类型
    /// </summary>
    [Required]
    [Comment("第三方用户类型")]
    public ExternalLoginChannel Type { get; set; }

    [Comment("昵称")]
    public string? NickName { get; set; }

    /// <summary>
    /// 名字（除姓氏以外的其他字）
    /// </summary>
    [Comment("名")]
    public string? GivenName { get; set; }

    /// <summary>
    /// 姓氏（姓名的第一个字）
    /// </summary>
    [Comment("姓")]
    public string? Surname { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [Comment("邮箱")]
    [StringLength(MaxLengths.Email)]
    public string? Email { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [Comment("性别")]
    public Gender Gender { get; set; }

    /// <summary>
    /// 头像 Url
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("头像 Url")]
    public string? AvatarUrl { get; set; }

    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Comment("更新时间")]
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// Steam Web API 用户密钥
    /// <para>https://steamcommunity.com/dev/apikey</para>
    /// </summary>
    [StringLength(MaxLengths.Url)]
    [Comment("Steam Web API 用户密钥")]
    public string? SteamWebAPIUserKey { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual List<UserDelete> UserDeletes { get; set; } = null!;

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<ExternalAccount>
    {
        public void Configure(EntityTypeBuilder<ExternalAccount> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.ExternalAccounts);

            builder.HasAlternateKey(x => new { x.ExternalAccountId, x.Type, });

            builder.HasOne(x => x.User)
                .WithMany(x => x.ExternalAccounts)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.UserDeletes)
                .WithMany(x => x.ExternalAccounts)
                .UsingEntity<UserDeleteExternalAccount>(x =>
                    x.HasOne(x => x.UserDelete)
                        .WithMany()
                        .HasForeignKey(x => x.UserDeleteId)
                        .OnDelete(DeleteBehavior.Cascade),
                    x => x.HasOne(x => x.ExternalAccount)
                        .WithMany()
                        .HasForeignKey(x => x.ExternalAccountId)
                        .OnDelete(DeleteBehavior.Cascade));
        }
    }

    public static Gender ParseGenderStr(string? gender)
    {
        if (!string.IsNullOrWhiteSpace(gender))
        {
            switch (gender.Length)
            {
                case 1:
                    {
                        switch (gender[0])
                        {
                            case '男': // QQ
                            case 'M': // Alipay
                            case 'm':
                            case '1': // Weixin
                                return Gender.Male;
                            case '女': // QQ
                            case 'F': // Alipay
                            case 'f':
                            case '2': // Weixin
                                return Gender.Female;
                        }
                    }
                    break;
                default:
                    {
                        if (Enum.TryParse(gender, true, out Gender result))
                        {
                            return result;
                        }
                    }
                    break;
            }
        }
        return Gender.Unknown;
    }
}
