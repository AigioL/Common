using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;

public interface IPCDbContextBase : IDbContextBase
{
    /// <summary>
    /// 从 Http 上下文中获取管理后台用户 Id
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    Guid? GetUserId(HttpContext? ctx);

    ///// <summary>
    ///// 从当前 Http 上下文中获取管理后台用户 Id
    ///// </summary>
    ///// <returns></returns>
    //Guid? GetCurrentUserId();

    static class TableNames
    {
        public const string Prefix = "PC";
        public const string Users = Prefix + "Users";
        public const string Roles = Prefix + "Roles";
        public const string RoleClaims = Prefix + "RoleClaims";
        public const string UserClaims = Prefix + "UserClaims";
        public const string UserLogins = Prefix + "UserLogins";
        public const string UserRoles = Prefix + "UserRoles";
        public const string UserTokens = Prefix + "UserTokens";
    }
}
