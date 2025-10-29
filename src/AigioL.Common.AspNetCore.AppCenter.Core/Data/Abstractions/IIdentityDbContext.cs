using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

public partial interface IIdentityDbContext : IDbContextBase
{
    #region 用户模块

    DbSet<User> Users { get; }

    DbSet<UserDelete> UserDeletes { get; }

    DbSet<UserDevice> UserDevices { get; }

    //DbSet<UserMessage> UserMessages { get; }

    DbSet<UserWallet> UserWallets { get; }

    DbSet<UserWalletChangeRecord> UserWalletChangeRecords { get; }

    //DbSet<UserExpRecord> UserExpRecords { get; }

    //DbSet<UserClockInRecord> UserClockInRecords { get; }

    DbSet<ExternalAccount> ExternalAccounts { get; }

    DbSet<UserDeleteExternalAccount> UserDeleteExternalAccounts { get; }

    DbSet<UserMembership> UserMemberships { get; }

    DbSet<UserMembershipChangeRecord> UserMembershipChangeRecords { get; }

    #endregion

    #region JsonWebToken

    DbSet<UserJsonWebToken> UserJsonWebTokens { get; }

    DbSet<UserRefreshJsonWebToken> UserRefreshJsonWebTokens { get; }

    #endregion
}