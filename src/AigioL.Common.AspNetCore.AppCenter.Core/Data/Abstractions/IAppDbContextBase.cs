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
    }
}
