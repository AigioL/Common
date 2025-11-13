using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public interface IAppVerRepository : IRepository<AppVer, Guid>, IEFRepository
{
    /// <summary>
    /// 获取全部版本信息
    /// </summary>
    Task<AppVer[]> GetAppVerAllAsync();
}
