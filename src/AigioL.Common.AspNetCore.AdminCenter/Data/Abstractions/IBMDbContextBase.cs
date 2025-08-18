namespace AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;

public interface IBMDbContextBase
{
    static class TableNames
    {
        public const string Users = "BMUsers";
        public const string Roles = "BMRoles";
        public const string RoleClaims = "BMRoleClaims";
        public const string UserClaims = "BMUserClaims";
        public const string UserLogins = "BMUserLogins";
        public const string UserRoles = "BMUserRoles";
        public const string UserTokens = "BMUserTokens";
    }
}
