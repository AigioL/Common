namespace AigioL.Common.AspNetCore.AppCenter.Constants;

/// <summary>
/// 缓存键字符串常量
/// </summary>
public static partial class CacheKeys
{
    /// <summary>
    /// Redis 活跃用户使用的 DB index
    /// </summary>
    public const int RedisActiveUserDb = 2;

    /// <summary>
    /// Redis 储存 Hash 类型数据 的 DB index
    /// </summary>
    public const int RedisHashDataDb = 5;


    public const string IdentityUserIsBanMapHashKey = "IdentityUserIsBanMapHashKey";
}