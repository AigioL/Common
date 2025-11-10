using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using static AigioL.Common.AspNetCore.AppCenter.Basic.Constants.BasicConstants;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class CustomerServiceController
{
    public static void MapBasicCustomerService(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "basic/customer/service")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("", async (HttpContext context) =>
        {
            var r = await ToCustomerService(context);
            return r;
        }).WithDescription("获取客服链接地址");
    }

    static readonly TimeSpan CustomerServiceUrlKeyExpiration = TimeSpan.FromMinutes(10);
    const bool useJwtUserIdOrUserId = true;

    /// <summary>
    /// 获取客服链接地址
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static async Task<ApiRsp<string?>> ToCustomerService(HttpContext context)
    {
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var repo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();

        var uid = useJwtUserIdOrUserId ? context.GetJwtUserIdThrowIfNull() : context.GetUserIdThrowIfNull();
        var result = await cache.GetOrCreateAsync(EKVP_CustomerServiceUrlKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CustomerServiceUrlKeyExpiration;
            var r = await repo.QueryAsync(EKVP_CustomerServiceUrlKey);
            return r;
        });
        if (result == null || string.IsNullOrWhiteSpace(result.Value))
        {
            // 尚未在管理后台中配置客服链接
            return HttpStatusCode.NotFound;
        }
        ApiRsp<string?> rsp = new()
        {
            Content = string.Format(result.Value, ShortGuid.Encode(uid)),
        };
        rsp.SetIsSuccess(true);
        return rsp;
    }
}
