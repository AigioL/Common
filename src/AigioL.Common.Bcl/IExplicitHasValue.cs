using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// HasValue 接口定义，通常显示实现此接口
/// </summary>
public interface IExplicitHasValue
{
    /// <summary>
    /// 判断 <see cref="IExplicitHasValue"/> 实例是否具有值
    /// </summary>
    bool ExplicitHasValue();
}

/// <summary>
/// 包含对 <see cref="IExplicitHasValue"/> 接口的扩展方法，用于判断对象是否具有值
/// </summary>
public static partial class ExplicitHasValueExtensions
{
    /// <summary>
    /// 判断指定的对象是否具有值
    /// </summary>
    /// <param name="obj">要判断的对象</param>
    /// <returns>如果对象不为 null 且具有值，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasValue([NotNullWhen(true)] this IExplicitHasValue? obj)
    {
        if (obj == null)
        {
            return false;
        }
        var hasValue = obj.ExplicitHasValue();
        return hasValue;
    }
}
