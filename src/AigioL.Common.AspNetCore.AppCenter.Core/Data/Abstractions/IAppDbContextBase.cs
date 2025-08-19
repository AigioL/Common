using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;

public interface IAppDbContextBase : IDbContextBase
{
    static class TableNames
    {
        public const string Users = "ACUsers";
        public const string Roles = "ACRoles";
        public const string RoleClaims = "ACRoleClaims";
        public const string UserClaims = "ACUserClaims";
        public const string UserLogins = "ACUserLogins";
        public const string UserRoles = "ACUserRoles";
        public const string UserTokens = "ACUserTokens";

        public const string UserDeletes = "ACUserDeletes";
        public const string UserDeleteExternalAccounts = "ACUserDeleteExternalAccounts";
        public const string UserMemberships = "ACUserMemberships";
        public const string UserMembershipChangeRecords = "ACUserMembershipChangeRecords";
        public const string UserJsonWebTokens = "ACUserJsonWebTokens";
        public const string UserRefreshJsonWebTokens = "ACUserRefreshJsonWebTokens";
        public const string UserWallets = "ACUserWallets";
        public const string UserWalletChangeRecords = "ACUserWalletChangeRecords";
        public const string ExternalAccounts = "ACExternalAccounts";
    }
}
