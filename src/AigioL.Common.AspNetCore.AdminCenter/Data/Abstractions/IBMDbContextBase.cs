using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;

public interface IBMDbContextBase : IDbContextBase
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
        public const string Users = "BMUsers";
        public const string Roles = "BMRoles";
        public const string RoleClaims = "BMRoleClaims";
        public const string UserClaims = "BMUserClaims";
        public const string UserLogins = "BMUserLogins";
        public const string UserRoles = "BMUserRoles";
        public const string UserTokens = "BMUserTokens";
    }
}
