using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 注销用户的外部账号
/// </summary>
[EntityTypeConfiguration(typeof(EntityTypeConfiguration))]
public partial class UserDeleteExternalAccount
{
    /// <summary>
    /// 注销用户 Id
    /// </summary>
    [Comment("注销用户 Id")]
    public Guid UserDeleteId { get; set; }

    /// <summary>
    /// 注销用户
    /// </summary>
    public virtual UserDelete UserDelete { get; set; } = null!;

    /// <summary>
    /// 外部账号表 Id
    /// </summary>
    [Comment("外部账号表 Id")]
    public Guid ExternalAccountId { get; set; }

    /// <summary>
    /// 外部账号
    /// </summary>
    public virtual ExternalAccount ExternalAccount { get; set; } = null!;

    // TODO: 改进为 ExternalAccount 记录表 record

    public sealed class EntityTypeConfiguration : IEntityTypeConfiguration<UserDeleteExternalAccount>
    {
        public void Configure(EntityTypeBuilder<UserDeleteExternalAccount> builder)
        {
            builder.ToTable(IAppDbContextBase.TableNames.UserDeleteExternalAccounts);
        }
    }
}
