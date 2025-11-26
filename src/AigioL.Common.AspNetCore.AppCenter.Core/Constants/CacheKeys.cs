namespace AigioL.Common.AspNetCore.AppCenter.Constants;

/// <summary>
/// 缓存键字符串常量
/// </summary>
public static partial class CacheKeys
{
    #region DB index

    /// <summary>
    /// Redis HashIncrement 使用的 DB index
    /// </summary>
    public const int RedisHashIncrementDb = 1;

    /// <summary>
    /// Redis 活跃用户使用的 DB index
    /// </summary>
    public const int RedisActiveUserDb = 2;

    /// <summary>
    /// Redis 锁使用的 DB index
    /// </summary>
    public const int RedisLockDb = 3;

    /// <summary>
    /// Redis 储存 Hash 类型数据 的 DB index
    /// </summary>
    public const int RedisHashDataDb = 5;

    /// <summary>
    /// Redis 消息队列的 DB index 用户 OpenId
    /// </summary>
    public const int RedisMessagingDb = 0; // InitQ 只支持默认 Db

    #endregion

    /// <summary>
    /// 广告列表
    /// </summary>
    public const string AdvertisementCacheKey =
        "AdvertisementCache";

    #region HashKey

    /// <summary>
    /// 广告图片地址 HashKey
    /// </summary>
    public const string AdvertisementImagesHashKey =
        "AdvertisementImagesHashKey";

    /// <summary>
    /// 广告跳转地址 HashKey
    /// </summary>
    public const string AdvertisementJumpHashKey =
        "AdvertisementJumpHashKey";

    /// <summary>
    /// 类型 UserDeviceIsTrustWithUserId 的缓存 HashKey
    /// </summary>
    public const string IdentityUserDeviceIsTrustWithUserIdMapHashKey =
        "IdentityUserDeviceIsTrustWithUserIdMapHashKey";

    /// <summary>
    /// 文章浏览量 HashKey
    /// </summary>
    public const string ArticleViewHashKey =
        "ArticleViewHashKey";

    public const string AppVersionHashKey =
        "AppVersionHashKey"; // 版本使用改缓存 Key 为 ID Last 最新与全部版本缓存 后台编辑时添加或编辑该缓存数据

    /// <summary>
    /// 缓存用户信息，减少数据库查询，在编辑、删除时清理该数据
    /// </summary>

    public const string IdentityUserInfoDataHashV1Key =
        "IdentityUserInfoDataHashKey_v1";

#if DEBUG
    [Obsolete("use IdentityUserDeviceIsTrustWithUserIdMapHashKey", true)]
    public const string IdentityUserJsonWebTokenInfoHashKey =
        IdentityUserDeviceIsTrustWithUserIdMapHashKey;
#endif

    public const string IdentityUserExternalAccountsHashKey =
        "IdentityUserExternalAccountsHashKey";

    #endregion

    public static string GetOrderUserRequestRefundMessageQueueKeyByBusinessType(int orderBusinessTypeId) => $"OrderBusinessType_{orderBusinessTypeId}";
}