using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IUserMembershipChangeRecordRepository
{
}

partial interface IUserMembershipChangeRecordRepository // 管理后台
{
    /// <summary>
    /// 后台表格查询
    /// </summary>
    /// <param name="userId">用户 Id</param>
    /// <param name="membershipChangeDirection">变更方向</param>
    /// <param name="memberLicenseType">会员订阅类型</param>
    /// <param name="isPayAsYoGo">是否为按量付费的时长</param>
    /// <param name="note">备注</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="current">当前页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <param name="cancellationToken"></param>
    /// <returns>分页模型</returns>
    Task<PagedModel<UserMembershipChangeRecordModel>> QueryAsync(
#if !USE_NUM_UID
        Guid? userId,
#else
        long? userId,
#endif
        MembershipChangeDirection? membershipChangeDirection,
        MembershipLicenseFlags? memberLicenseType,
        bool? isPayAsYoGo,
        string? note,
        DateTimeOffset?[]? createTime,
        int current,
        int pageSize,
        CancellationToken cancellationToken = default);
}
