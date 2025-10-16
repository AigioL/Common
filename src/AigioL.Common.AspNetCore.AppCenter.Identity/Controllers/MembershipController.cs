using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class MembershipController
{
    /// <summary>
    /// 获取会员信息
    /// </summary>
    /// <returns></returns>
    static async Task<ApiRsp<TMembershipInfo?>> GetUserMembershipInfo<TMembershipInfo>(
        HttpContext context)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //var user = HttpContext.GetUserId();
        //if (user == null)
        //    return ApiRspCode.Unauthorized;

        //var membershipInfo = await GetCacheData(user.Value, connection, () => userMembershipRepository.GetUserMembershipAsync(user.Value));
        //return membershipInfo;
    }
}
