using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities.Net;
using AigioL.Common.AspNetCore.AppCenter.Models.Net;
using AigioL.Common.AspNetCore.AppCenter.Repositories.Net.Abstractions;
using AigioL.Common.Extensions.Http.Proxy.Models;
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
    public async Task<WebProxyModel[]> GetWebProxiesAsync(
        byte? groupId = null,
        CancellationToken cancellationToken = default)
    {
        var q = from e in dbContext.WebProxys
                where e.Disable == false
                select e;
        if (groupId.HasValue)
        {
            q = q.Where(e => e.GroupId == groupId.Value);
        }
        var q2 = from e in q
                 let isNetworkCredential = e.CredentialsType == WebProxyCredentialsType.NetworkCredential
                 //let isCredentialCache = e.CredentialsType == WebProxyCredentialsType.CredentialCache
                 let isNull = e.CredentialsType == WebProxyCredentialsType.Null
                 where isNetworkCredential || isNull
                 select new WebProxyModel
                 {
                     Id = e.Id,
                     Address = e.Address,
                     BypassList = e.BypassList,
                     BypassProxyOnLocal = e.BypassProxyOnLocal,
                     UseDefaultCredentials = e.UseDefaultCredentials,
                     Credentials = isNetworkCredential ? new NetworkCredentialModel
                     {
                         Domain = e.Domain,
                         Password = e.Password,
                         UserName = e.UserName,
                     } : null,
                 };
        var r = await q2.ToArrayAsync(cancellationToken);
        return r;
    }
}
