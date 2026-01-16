using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities.Net;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Net.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Repositories.Net;

public sealed partial class WebProxyRepository<TDbContext>(
    TDbContext dbContext,
    IServiceProvider serviceProvider) :
#pragma warning disable CS9107 // 参数捕获到封闭类型状态，其值也传递给基构造函数。该值也可能由基类捕获。
    Repository<TDbContext, WebProxyEntity, string>(dbContext, serviceProvider),
#pragma warning restore CS9107 // 参数捕获到封闭类型状态，其值也传递给基构造函数。该值也可能由基类捕获。
    IWebProxyRepository
    where TDbContext : DbContext, IDbContextBase, IKeyValuePairsDbContext
{
}
