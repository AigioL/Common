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
    /// Redis 储存 Hash 类型数据 的 DB index
    /// </summary>
    public const int RedisHashDataDb = 5;

    #endregion

    #region HashKey

    public const string IdentityUserIsBanMapHashKey = "IdentityUserIsBanMapHashKey";

    /// <summary>
    /// 文章浏览量 HashKey
    /// </summary>
    public const string ArticleViewHashKey =
        "ArticleViewHashKey";

    public const string AppVersionHashKey = "AppVersionHashKey"; // 版本使用改缓存 Key 为 ID Last 最新与全部版本缓存 后台编辑时添加或编辑该缓存数据

    #endregion
}