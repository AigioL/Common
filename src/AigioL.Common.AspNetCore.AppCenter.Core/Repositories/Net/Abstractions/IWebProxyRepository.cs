using AigioL.Common.AspNetCore.AppCenter.Entities.Net;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Repositories.Net.Abstractions;

public partial interface IWebProxyRepository : IRepository<WebProxyEntity, string>, IEFRepository
{
    const string CacheKey = "WebProxys";
}
