using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Primitives.Columns;
using MemoryPack;
using Microsoft.Extensions.Primitives;
using StackExchange.Redis;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Services;

partial class AppVerCoreService
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "解析 RSA Padding 失败，值为：{rsaPaddingStr}, UserAgent: {userAgent}")]
    protected static partial void LogErrorGetRSAPadding(ILogger logger, Exception e, string? rsaPaddingStr, StringValues userAgent);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "解密 AES 密钥(Hex) 失败, UserAgent: UserAgent: {userAgent}")]
    protected static partial void LogErrorRSADecryptHex(ILogger logger, Exception e, StringValues userAgent);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "解密 AES 密钥(Base64Url) 失败, UserAgent: UserAgent: {userAgent}")]
    protected static partial void LogErrorRSADecryptBase64Url(ILogger logger, Exception e, StringValues userAgent);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "创建 AES 密钥失败, UserAgent: UserAgent: {userAgent}")]
    protected static partial void LogErrorAesCreate(ILogger logger, Exception e, StringValues userAgent);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "版本号主键: {appVersionInfoString}, 没有正确的私钥")]
    protected static partial void LogErrorPrivateKeyIsNullOrWhiteSpace(
        ILogger logger,
        string? appVersionInfoString);
}

sealed partial class AppVerCoreService<TDbContext> : AppVerCoreService,
    IAppVerCoreService
    where TDbContext : IAppVerDbContext
{
    readonly TDbContext db;
    readonly ILogger logger;
    readonly IConnectionMultiplexer connection;

#pragma warning disable IDE0290 // 使用主构造函数
    public AppVerCoreService(
#pragma warning restore IDE0290 // 使用主构造函数
        ILogger<AppVerCoreService<TDbContext>> logger,
        IConnectionMultiplexer connection,
        TDbContext db,
        IHttpContextAccessor httpContextAccessor)
    {
        this.logger = logger;
        this.connection = connection;
        this.db = db;
    }

    const string SecurityResultKey = "SecurityResult";

    /// <summary>
    /// (Hex)经过 RSA 加密的 AES 密钥
    /// </summary>
    const string KeyAESKeyHex = "sKeyHex";

    /// <summary>
    /// (Base64Url)经过 RSA 加密的 AES 密钥
    /// </summary>
    const string KeyAESKey = "sKey";

    /// <summary>
    /// RSA Padding
    /// </summary>
    const string KeyRSAPadding = "sKeyPadding";

    public async Task<(string? ErrorMessage, Aes? Aes)> GetSecurityResultAsync(HttpContext context)
    {
        if (context.Items[SecurityResultKey] is not ValueTuple<string?, Aes?> result)
        {
            result = await GetSecurityResultCoreAsync(context);
            context.Items[SecurityResultKey] = result;
        }
        return result;
    }

    async Task<(string? ErrorMessage, Aes? Aes)> GetSecurityResultCoreAsync(HttpContext context)
    {
        #region 获取当前的 App 版本数据，拿到 RSA 私钥

        var appVersionInfo = await GetAsync(context, fromHeaderOrQuery: false);
        if (appVersionInfo == null)
        {
            return ("Failed to Unknown app version.", default);
        }

        #endregion 获取当前的 App 版本数据，拿到 RSA 私钥

        #region 从 URL Query 中获取 RSAEncryptionPadding

        string? rsaPaddingStr = null;
        RSAEncryptionPadding rsaPadding;
        try
        {
            rsaPaddingStr = context.GetQueryOrSessionValue(KeyRSAPadding);
            rsaPadding = RSAUtils.GetPaddingByOaepHashAlgorithmName(rsaPaddingStr);
        }
        catch (Exception e)
        {
            LogErrorGetRSAPadding(logger, e, rsaPaddingStr, context.Request.Headers.UserAgent);
            return ("Failed to resolve key padding.", default);
        }

        #endregion 从 URL Query 中获取 RSAEncryptionPadding

        var rsaPara = RSAUtils.GetRSAParameters(appVersionInfo.PrivateKey);
        if (!rsaPara.HasValue)
        {
            return ("Failed to Unknown app key.", default);
        }
        using var rsa = RSA.Create(rsaPara.Value);

        byte[]? aesKeyBytes = null;
        var aesKeyHex = context.GetQueryOrSessionValue(KeyAESKeyHex);
        if (!string.IsNullOrEmpty(aesKeyHex))
        {
            try
            {
                aesKeyBytes = rsa.DecryptToByteArrayHex(aesKeyHex, rsaPadding);
            }
            catch (Exception e)
            {
                LogErrorRSADecryptHex(logger, e, context.Request.Headers.UserAgent);
                return ("Failed to decrypt security hex key.", default);
            }
        }
        else
        {
            var aesKey = context.GetQueryOrSessionValue(KeyAESKey);
            if (!string.IsNullOrEmpty(aesKey))
            {
                try
                {
                    aesKeyBytes = rsa.DecryptToByteArray(aesKey, rsaPadding);
                }
                catch (Exception e)
                {
                    LogErrorRSADecryptBase64Url(logger, e, context.Request.Headers.UserAgent);
                    return ("Failed to decrypt security b64u key.", default);
                }
            }
            else
            {
                return ("Failed to Security key not found.", default);
            }
        }

        if (aesKeyBytes == null)
        {
            return ("Failed to Security key is null.", default);
        }

        try
        {
            var aes = AESUtils.Create(aesKeyBytes);
            return (default, aes);
        }
        catch (Exception e)
        {
            LogErrorAesCreate(logger, e, context.Request.Headers.UserAgent);
            return ("Failed to create a.", default);
        }
    }

    public async ValueTask<IReadOnlyAppVer?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await db.AppVers.FindAsync([id], cancellationToken: cancellationToken);
        return r;
    }

    public async Task<IReadOnlyAppVer?> GetAsync(HttpContext context, bool fromHeaderOrQuery)
    {
        IReadOnlyAppVer? appVersionInfo = null;

        if (fromHeaderOrQuery)
        {
            string? api_referrer = context.Request.Headers.GetCustomReferrer().ToString();
            if (!string.IsNullOrWhiteSpace(api_referrer))
            {
                var api_referrer_span = api_referrer.AsSpan();
                var split = api_referrer_span.Split('/');
                Range? lastSplit = default;
                while (split.MoveNext())
                {
                    lastSplit = split.Current;
                }
                string? appVersionValue;
                if (lastSplit.HasValue)
                {
                    appVersionValue = new(api_referrer_span[lastSplit.Value]);
                }
                else
                {
                    appVersionValue = null;
                }
                appVersionInfo = await CheckHttpRequestHeadersByValueAsync(
                    context, appVersionValue);
            }

            if (appVersionInfo == null)
            {
                var appVersionId = context.Request.Headers[ApiConstants.Headers_AppVersion];
                if (!string.IsNullOrWhiteSpace(appVersionId))
                {
                    appVersionInfo = await CheckHttpRequestHeadersByGuidIdAsync(
                        context, appVersionId);
                }
            }
        }
        else
        {
            var ver = context.GetQueryOrSessionValue("ver");
            if (!string.IsNullOrWhiteSpace(ver))
            {
                appVersionInfo = await CheckHttpRequestHeadersByBase64ValueAsync(
                    context, ver);
            }

            if (appVersionInfo == null)
            {
                var version = context.GetQueryOrSessionValue("version");
                if (!string.IsNullOrWhiteSpace(version))
                {
                    appVersionInfo = await CheckHttpRequestHeadersByGuidIdAsync(
                        context, version);
                }
            }
        }

        return appVersionInfo;
    }

    public Task<IReadOnlyAppVer?> GetAsync(string appVersion, CancellationToken cancellationToken = default)
        => GetByIdOrValueAsync(null, appVersion, cancellationToken);
}

partial class AppVerCoreService<TDbContext> // 内部函数
{
    IReadOnlyAppVer? CheckHttpRequestHeaders(HttpContext context, IReadOnlyAppVer? appVersionInfo)
    {
        if (appVersionInfo == null)
        {
            // 请求头 APP 版本号 GUID 主键在数据库中找不到
            return null;
        }

        if (appVersionInfo.Disable)
        {
            // 该版本已被禁用
            context.Response.Headers.Append(ApiConstants.Headers_AppObsolete, "true");
            return null;
        }

        if (string.IsNullOrWhiteSpace(appVersionInfo.PrivateKey))
        {
            // 发行新版本在数据库中的值不对
            var appVersionInfoString = appVersionInfo.ToString();
            LogErrorPrivateKeyIsNullOrWhiteSpace(logger, appVersionInfoString);
            return null;
        }

        context.Items[ApiConstants.Headers_AppVersion] = appVersionInfo;

        return appVersionInfo;
    }

    async Task<IReadOnlyAppVer?> GetByIdOrValueAsync(
        Guid? appVersionId,
        string? appVersionValue,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyAppVer? appVersionInfo = null;
        var redisdb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
        if (appVersionId.HasValue)
        {
            var appVersionIdS = ShortGuid.Encode(appVersionId.Value);
            var cacheData = await redisdb.HashGetAsync(CacheKeys.AppVersionHashKey, appVersionIdS);
            if (cacheData.HasValue)
            {
                appVersionInfo = MemoryPackSerializer.Deserialize<AppVerRedisModel>((byte[])cacheData!);
            }
            //await Entity.AsNoTrackingWithIdentityResolution()
            //.FirstOrDefaultAsync(x => x.Id == appVersionId.Value, cancellationToken);
        }
        else if (appVersionValue != null)
        {
            if (appVersionValue.Length <= MaxLengths.Version && Version.TryParse(appVersionValue, out var appVersionValue_))
            {
                var cacheData = await redisdb.HashGetAsync(CacheKeys.AppVersionHashKey, appVersionValue);
                if (cacheData.HasValue)
                {
                    appVersionInfo = MemoryPackSerializer.Deserialize<AppVerRedisModel>((byte[])cacheData!);
                }
                //appVersionInfo = await Entity.AsNoTrackingWithIdentityResolution()
                //    .FirstOrDefaultAsync(x => x.Version == appVersionValue, cancellationToken);
                if (appVersionInfo == null)
                {
                    cacheData = await redisdb.HashGetAsync(CacheKeys.AppVersionHashKey, "GetLastOrDefaultAsync");
                    if (cacheData.HasValue)
                    {
                        appVersionInfo = MemoryPackSerializer.Deserialize<AppVerRedisModel>((byte[])cacheData!);
                    }
                    // 版本号值找不到数据库纪录，获取最新的将版本号值改为传入的，Id 清空，返回
                    //appVersionInfo = await GetLastOrDefaultAsync(cancellationToken);
                    appVersionInfo?.SetIdVersion(default, appVersionValue);
                }
            }
        }
        if (appVersionInfo == null)
        {
            // 请求头 APP 版本号 GUID 主键在数据库中找不到
            return null;
        }
        return appVersionInfo;
    }

    /// <summary>
    /// 根据 HTTP 请求头内容检测当前版本号(版本号字符串值)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    async Task<IReadOnlyAppVer?> CheckHttpRequestHeadersByValueAsync(HttpContext context, string? value)
    {
        var appVersionInfo = await GetByIdOrValueAsync(null, value, context.RequestAborted);
        var r = CheckHttpRequestHeaders(context, appVersionInfo);
        return r;
    }

    /// <summary>
    /// 根据 HTTP 请求头内容检测当前版本号(版本号 Guid)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    async Task<IReadOnlyAppVer?> CheckHttpRequestHeadersByGuidIdAsync(HttpContext context, string? value)
    {
        Guid? appVersionId = null;
        if (ShortGuid.TryParse(value, out Guid versionId))
        {
            appVersionId = versionId;
        }
        var appVersionInfo = await GetByIdOrValueAsync(appVersionId, null, context.RequestAborted);
        var r = CheckHttpRequestHeaders(context, appVersionInfo);
        return r;
    }

    /// <summary>
    /// 根据 HTTP 请求头内容检测当前版本号(Base64Url 版本号字符串值)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<IReadOnlyAppVer?> CheckHttpRequestHeadersByBase64ValueAsync(HttpContext context, string? value)
    {
        string? appVersionValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            appVersionValue = null;
        }
        else
        {
            var appVersionValueU8 = Base64Url.DecodeFromChars(value);
            appVersionValue = Encoding.UTF8.GetString(appVersionValueU8);
        }
        var r = CheckHttpRequestHeadersByValueAsync(context, appVersionValue);
        return r;
    }
}