using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.AnalysisLogs;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
using AigioL.Common.Repositories.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;

public partial interface IAnalysisLogRepository : IRepository<AnalysisEventLog, Guid>
{
}

partial interface IAnalysisLogRepository
{
    Task<IdNameModel[]?> GetAppIdListAsync(CancellationToken cancellationToken = default);
}