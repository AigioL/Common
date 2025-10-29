using AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class OfficialMessageController
{
    public static void MapBasicOfficialMessage(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "basic/officialmessage")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("message", async (HttpContext context,
            [FromQuery] ClientPlatform? clientPlatform,
            [FromQuery] OfficialMessageType? messageType,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var r = await Get(context, clientPlatform, messageType, current, pageSize);
            return r;
        }).WithDescription("获取官方消息");
    }

    /// <summary>
    /// 官方消息数据内存中缓存过期时间 1 分钟
    /// </summary>
    const int official_message_memory_timeout_minutes = 1;

    /// <summary>
    /// 获取官方消息
    /// </summary>
    /// <param name="context"></param>
    /// <param name="clientPlatform">客户端平台</param>
    /// <param name="messageType">官方消息类型</param>
    /// <param name="current">当前页码</param>
    /// <param name="pageSize">页大小</param>
    /// <returns></returns>
    static async Task<ApiRsp<PagedModel<OfficialMessageItemModel>?>> Get(
        HttpContext context,
        ClientPlatform? clientPlatform,
        OfficialMessageType? messageType,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize)
    {
        var appVer = await context.GetAppVerAsync();
        var cacheKey = $"{nameof(OfficialMessageController)}_message_{current}_{pageSize}_{appVer?.Version}_{clientPlatform}_{messageType}";
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(official_message_memory_timeout_minutes);
            var repo = context.RequestServices.GetRequiredService<IOfficialMessageRepository>();
            var r = await repo.QueryAsync(appVer, clientPlatform, messageType, current, pageSize);
            return r;
        });
        return result;
    }
}
