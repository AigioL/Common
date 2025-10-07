using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.Essentials.Storage;

/// <summary>
/// 首选项 API 有助于将应用程序首选项存储在键/值存储中
/// </summary>
public partial interface IPreferences
{
    /// <summary>
    /// 清除所有键和值
    /// </summary>
    /// <param name="sharedName">共享容器名称</param>
    void Clear(string? sharedName = default);

    /// <summary>
    /// 检查是否存在给定密钥
    /// </summary>
    /// <param name="key">要检查的键</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <returns>如果首选项中存在键，则为 <see langword="true"/>，否则为 <see langword="false"/></returns>
    bool ContainsKey(string key, string? sharedName = default);

    /// <summary>
    /// 获取给定键的值，如果键不存在，则获取指定的默认值
    /// </summary>
    /// <typeparam name="T">为此首选项存储的对象类型</typeparam>
    /// <param name="key">要检索其值的键</param>
    /// <param name="defaultValue">不存在的现有 key 值时要返回的默认值</param>
    /// <param name="sharedName">共享容器名称</param>
    /// <returns>给定键的值，如果不存在，则为 defaultValue 的值</returns>
    T? Get<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? defaultValue = default, string? sharedName = default);

    /// <summary>
    /// 删除键及其关联的值（如果存在）
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <param name="sharedName">共享容器名称</param>
    void Remove(string key, string? sharedName = default);

    /// <summary>
    /// 为给定键设置一个值
    /// </summary>
    /// <typeparam name="T">存储在此首选项中的对象的类型</typeparam>
    /// <param name="key">要为其设置值的键</param>
    /// <param name="value">要设置的值</param>
    /// <param name="sharedName">共享容器名称</param>
    void Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? value, string? sharedName = default);
}
