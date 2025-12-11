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
    /// Redis 访问 Token DB index
    /// </summary>
    public const int RedisAccessTokenDb = 4;

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

    /// <summary>
    /// Steam 充值用户操作并发锁
    /// </summary>
    public static string GetSteamRechargeUserOperationLockKey(Guid userId) => $"SteamRechargeUserOperationHashKey:{userId:N}";

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

    /// <summary>
    /// 支付服务是否停止
    /// </summary>
    public const string PaymentServiceStopped = "PaymentServiceStopped";

    #region 用户会员

    /// <summary>
    /// 获取用户会员信息缓存 Key
    /// </summary>
    public static string GetUserMembershipCacheKey(Guid userId) => $"UserMembership:{userId}";

    /// <summary>
    /// 获取用户会员信息缓存锁 Key
    /// </summary>
    public static string GetUserMembershipCacheLockKey(Guid userId) => $"UserMembershipLock:{userId}";

    /// <summary>
    /// 获取会员商品信息缓存 Key
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public const string GetMembershipGoodsCacheKey = "MembershipGoodsCacheKey";

    #endregion

    public static string GetOrderUserRequestRefundMessageQueueKeyByBusinessType(int orderBusinessTypeId) => $"OrderBusinessType_{orderBusinessTypeId}";
}