using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using MemoryPack;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Controllers;

/// <summary>
/// 活跃用户匿名统计
/// </summary>
public static class ActiveUsersController
{
    public static void MapAnalyticsActiveUsers(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "analysis/activeusers")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] ActiveUserRecordModel m) =>
        {
            var r = await PostAsync(context, m);
            return r;
        });

    }

    /// <summary>
    /// 根据当天去重插入统计，一天一个设备仅记录一次
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="deviceId"></param>
    /// <param name="platform"></param>
    /// <returns></returns>
    static async Task<bool> ContainsAsync(
        IConnectionMultiplexer connection,
        string deviceId,
#pragma warning disable CS0618 // 类型或成员已过时
        WebApiCompatDevicePlatform platform)
#pragma warning restore CS0618 // 类型或成员已过时
    {
        var db = connection.GetDatabase(CacheKeys.RedisActiveUserDb);
        var key = $"{DateTime.Today:yyyy-MM-dd}{platform}";
        var contains = await db.SetContainsAsync(key, deviceId);
        if (contains)
        {
            return true;
        }
        else
        {
            await db.SetAddAsync(key, deviceId);
            return false;
        }
    }

    static async Task<ApiRsp> PostAsync(HttpContext context, ActiveUserRecordModel model)
    {
        var appVer = await context.GetAppVerAsync();
        if (appVer == null)
        {
            return HttpStatusCode.BadRequest;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(ip))
        {
            return HttpStatusCode.BadRequest;
        }

        var deviceId = model.GetDeviceId();
        if (deviceId == null)
        {
            return HttpStatusCode.BadRequest;
        }

        if (model.OSVersion == null || model.OSVersion.Length > MaxLengths.Version)
        {
            return HttpStatusCode.BadRequest;
        }

        Guid? channelPackageIdGN = null;
        if (!string.IsNullOrWhiteSpace(model.ChannelPackageId))
        {
            var channelPackageService = context.RequestServices.GetService<IChannelPackageService>();
            if (!IChannelPackageService.CheckId(
                    channelPackageService,
                    model.ChannelPackageId,
                    out channelPackageIdGN,
                    out ApiRspCode code))
            {
                return code;
            }
            if (channelPackageIdGN.HasValue)
            {
                var exists = await channelPackageService.ExistsAsync(channelPackageIdGN.Value, context.RequestAborted);
                if (!exists)
                {
                    // 渠道包 Id 不存在
                    return ApiRspCode.NotFound;
                }
            }
        }

        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var contains = await ContainsAsync(connection, deviceId, model.Platform);
        if (contains)
        {
            return HttpStatusCode.OK;
        }

        ActiveUserAnonymousStatisticCacheModel cacheModel = new()
        {
            Model = model,
            IPAddress = ip,
            DevicePlatform = context.GetDevicePlatform(),
            DeviceId = deviceId,
            AppVersion = appVer.Version,
            ChannelPackageId = channelPackageIdGN,
        };
        const string k = nameof(ActiveUserAnonymousStatisticCacheModel);
        var v = MemoryPackSerializer.Serialize(cacheModel);
        var dbConnection = connection.GetDatabase(CacheKeys.RedisHashDataDb);
        await dbConnection.ListRightPushAsync(k, v);

        return HttpStatusCode.OK;
    }
}
