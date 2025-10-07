using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.Essentials.Storage;

/// <summary>
/// SecureStorage API 有助于安全地存储简单的键/值对
/// </summary>
public partial interface ISecureStorage
{
    /// <summary>
    /// 获取并解密给定密钥的值
    /// </summary>
    /// <typeparam name="T">为此的对象类型</typeparam>
    /// <param name="key">要检索其值的键</param>
    /// <param name="defaultValue">不存在的现有 key 值时要返回的默认值</param>
    /// <returns>解密的值，如果未找到值，则为 <see langword="null"/></returns>
    Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? defaultValue = default);

    /// <summary>
    /// 删除键及其关联的值（如果存在）
    /// </summary>
    /// <param name="key">要移除的键</param>
    /// <returns></returns>
    bool Remove(string key);

    /// <summary>
    /// 删除所有存储的加密密钥/值对
    /// </summary>
    void RemoveAll();

    /// <summary>
    /// 设置并加密给定密钥的值
    /// </summary>
    /// <typeparam name="T">存储的对象的类型</typeparam>
    /// <param name="key">要为其设置值的键</param>
    /// <param name="value">要设置的值</param>
    /// <returns>具有 <see cref="Task"/> 异步操作的当前状态的对象</returns>
    Task SetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? value);
}
