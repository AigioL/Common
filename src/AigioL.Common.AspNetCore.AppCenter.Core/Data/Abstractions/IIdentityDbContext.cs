using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions
{
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
}

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IIdentityDbContext
    {
        #region 用户模块

        DbSet<User> IIdentityDbContext.Users => ACUsers;

        public DbSet<UserDelete> ACUserDeletes { get; set; } = null!;

        DbSet<UserDelete> IIdentityDbContext.UserDeletes => ACUserDeletes;

        public DbSet<UserDevice> ACUserDevices { get; set; } = null!;

        DbSet<UserDevice> IIdentityDbContext.UserDevices => ACUserDevices;

        public DbSet<UserWallet> ACUserWallets { get; set; } = null!;

        DbSet<UserWallet> IIdentityDbContext.UserWallets => ACUserWallets;

        public DbSet<UserWalletChangeRecord> ACUserWalletChangeRecords { get; set; } = null!;

        DbSet<UserWalletChangeRecord> IIdentityDbContext.UserWalletChangeRecords => ACUserWalletChangeRecords;

        public DbSet<ExternalAccount> ACExternalAccounts { get; set; } = null!;

        DbSet<ExternalAccount> IIdentityDbContext.ExternalAccounts => ACExternalAccounts;

        public DbSet<UserDeleteExternalAccount> ACUserDeleteExternalAccounts { get; set; } = null!;

        DbSet<UserDeleteExternalAccount> IIdentityDbContext.UserDeleteExternalAccounts => ACUserDeleteExternalAccounts;

        public DbSet<UserMembership> ACUserMemberships { get; set; } = null!;

        DbSet<UserMembership> IIdentityDbContext.UserMemberships => ACUserMemberships;

        public DbSet<UserMembershipChangeRecord> ACUserMembershipChangeRecords { get; set; } = null!;

        DbSet<UserMembershipChangeRecord> IIdentityDbContext.UserMembershipChangeRecords => ACUserMembershipChangeRecords;

        #endregion

        #region JsonWebToken

        public DbSet<UserJsonWebToken> ACUserJsonWebTokens { get; set; } = null!;

        DbSet<UserJsonWebToken> IIdentityDbContext.UserJsonWebTokens => ACUserJsonWebTokens;

        public DbSet<UserRefreshJsonWebToken> ACUserRefreshJsonWebTokens { get; set; } = null!;

        DbSet<UserRefreshJsonWebToken> IIdentityDbContext.UserRefreshJsonWebTokens => ACUserRefreshJsonWebTokens;

        #endregion
    }
}
#endif