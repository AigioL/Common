using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;
using Microsoft.Extensions.Options;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using static AigioL.Common.AspNetCore.AppCenter.Payment.Jobs.LogRefreshWeChatAccessTokenJob;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Jobs;

/// <summary>
/// 刷新微信 AccessToken 任务
/// </summary>
public partial class RefreshWeChatAccessTokenJob<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings>(
    IConnectionMultiplexer redisConnection,
    IOptions<TAppSettings> options,
    ILogger<RefreshWeChatAccessTokenJob<TAppSettings>> logger,
    AppDbContext dbContext,
    IFeishuApiClient feishuApiClient) :
    JobService<AppDbContext, RefreshWeChatAccessTokenJob<TAppSettings>>(logger, dbContext, feishuApiClient)
    where TAppSettings : class, IWeChatApiAppSettings
{
    protected sealed override async Task<ApiRsp> HandleAsync(IJobExecutionContext? context, CancellationToken cancellationToken)
    {
        string? appIdWeChat, appSecretWeChat;

        try
        {
            appIdWeChat = options.Value.WeChatApiOptions?.AppId;
            appSecretWeChat = options.Value.WeChatApiOptions?.AppSecret;
        }
        catch (ArgumentNullException)
        {
            appIdWeChat = appSecretWeChat = null;
        }
        if (string.IsNullOrWhiteSpace(appIdWeChat) || string.IsNullOrWhiteSpace(appSecretWeChat))
        {
            logger.LogWarning("获取 WeChatApiOptions.AppId 为空");
            return true;
        }

        try
        {
            const string redisKey = "AccessToken";
            var redisHashField = $"{nameof(PaymentAccessTokenEnum.WeiXinAccessToken)}:{appIdWeChat}";
            var redis = redisConnection.GetDatabase(CacheKeys.RedisAccessTokenDb);
            ReadOnlySpan<char> cache = await redis.HashGetAsync(redisKey, redisHashField);
            var accessToken = cache.Length > 0 ? JsonSerializer.Deserialize(cache, PaymentMinimalApisJsonSerializerContext.Default.WeChatAccessToken) : null;
            if (accessToken == null || accessToken.ExpireTimestamp <= DateTimeOffset.Now.ToUnixTimeSeconds())
            {
                var client = CreateWeXinClient(appIdWeChat, appSecretWeChat);
                var request = new CgibinStableTokenRequest();
                var response = await client.ExecuteCgibinStableTokenAsync(request, cancellationToken);
                if (!response.IsSuccessful())
                {
                    LogErrorExecuteCgibinStableToken(logger,
                        appIdWeChat,
#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
                        response.GetRawStatus(),
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
                        response.ErrorCode,
                        response.ErrorMessage);
                }
                else
                {
                    // 提前十分钟过期，以便于系统能及时刷新，防止因在过期临界点时出现问题
                    var now = DateTimeOffset.Now;
                    long nextExpireTimestamp = now.AddSeconds(response.ExpiresIn)
                        .AddMinutes(-10d)
                        .ToUnixTimeSeconds();
                    accessToken ??= new()
                    {
                        CreateTimestamp = now.ToUnixTimeSeconds(),
                    };
                    accessToken.AppId = appIdWeChat;
                    accessToken.AppType = unchecked((int)PaymentAccessTokenEnum.WeiXinAccessToken);
                    accessToken.AccessToken = response.AccessToken;
                    accessToken.ExpireTimestamp = nextExpireTimestamp;
                    accessToken.UpdateTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                    var accessTokenJson = JsonSerializer.Serialize(accessToken, PaymentMinimalApisJsonSerializerContext.Default.WeChatAccessToken);
                    await redis.HashSetAsync(redisKey, redisHashField, accessTokenJson);

                    logger.LogInformation("已定时刷新获取微信 AccessToken，有效期至：{nextExpireTimestamp}", nextExpireTimestamp);
                }
            }
            else
            {
                logger.LogInformation("无需刷新 AccessToken，AccessToken 未过期");
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "定时刷新获取微信 AccessToken 任务异常");
            throw;
        }
    }

    protected virtual WechatApiClient CreateWeXinClient(string appIdWeChat, string appSecretWeChat)
    {
        var wechatApiClient = new WechatApiClient(new()
        {
            AppId = appIdWeChat,
            AppSecret = appSecretWeChat,
        });

        wechatApiClient.Configure(settings =>
        {
            settings.JsonSerializer = new global::SKIT.FlurlHttpClient.SystemTextJsonSerializer();
        });

        return wechatApiClient;
    }
}

static partial class LogRefreshWeChatAccessTokenJob
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "刷新 AppId 为 {appIdWeChat} 微信 AccessToken 失败，状态码：{rawStatus}，错误代码：{errorCode}，错误描述：{errorMessage}")]
    internal static partial void LogErrorExecuteCgibinStableToken(ILogger logger, string appIdWeChat, int rawStatus, int errorCode, string? errorMessage);
}