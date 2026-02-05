using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.ActiveUsers;
using AigioL.Common.Repositories.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;

public interface IActiveUserRecordRepository : IRepository<ActiveUserAnonymousStatistic, Guid>
{
    /// <summary>
    /// 批量插入实体
    /// </summary>
    Task BulkInsertAsync(params IEnumerable<ActiveUserAnonymousStatistic> activeUsers);
}
