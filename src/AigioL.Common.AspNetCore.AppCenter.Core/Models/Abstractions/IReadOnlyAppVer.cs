using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

/// <summary>
/// APP 版本号信息只读接口
/// </summary>
public interface IReadOnlyAppVer : ICreationTime
{
    /// <summary>
    /// 版本号
    /// </summary>
    string Version { get; set; }

    /// <summary>
    /// 私钥
    /// </summary>
    string PrivateKey { get; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    bool Disable { get; }

    /// <summary>
    /// 主键 Id
    /// </summary>
    Guid Id
    {
        [Obsolete("如果版本号值为新的或不在数据库中，则此值为默认值，通常不应使用该 Id 与其他表关联或类似用途")]
        get;
        set;
    }

    /// <summary>
    /// 设置 Id 和 Version 值
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version"></param>
    void SetIdVersion(Guid id, string version)
    {
        Id = id;
        Version = version;
    }
}