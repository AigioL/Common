using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// ThrowHelper 类是可用于高效引发异常的帮助程序类型。 它旨在支持 Guard API，并且应主要用于开发人员需要对引发的异常类型进行精细控制的情况，或者需要对要包含的确切异常消息进行精细控制。
/// </summary>
public static partial class ThrowHelper
{
    /// <summary>
    /// 抛出 <see cref="ArgumentNullException"/>
    /// </summary>
    /// <param name="paramName"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [DoesNotReturn]
    public static void ThrowArgumentNullException(string? paramName)
        => throw new ArgumentNullException(paramName);

    /// <summary>
    /// 抛出异常 <see cref="ArgumentOutOfRangeException(string?, object?, string?)"/>，使用通用 message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="actualValue"></param>
    /// <param name="paramName"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException<T>(T actualValue, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null) =>
          throw new ArgumentOutOfRangeException(string.Format(SR.Arg_ArgumentOutOfRangeException__, paramName, actualValue));

    /// <summary>
    /// 创建异常 <see cref="ArgumentOutOfRangeException(string?, object?, string?)"/>，使用通用 message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="actualValue"></param>
    /// <param name="paramName"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArgumentOutOfRangeException GetArgumentOutOfRangeException<T>(T actualValue, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null) =>
        new(string.Format(SR.Arg_ArgumentOutOfRangeException__, paramName, actualValue));

    /// <summary>
    /// 创建异常 <see cref="ArgumentOutOfRangeException(string?, object?, string?)"/>，使用自定义 message
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="actualValue"></param>
    /// <param name="message"></param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArgumentOutOfRangeException GetArgumentOutOfRangeWithMessageException<T>(T actualValue, string message, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null) =>
        new(string.Format(SR.Arg_ArgumentOutOfRangeException___, paramName, actualValue, message));
}

file static partial class SR
{
    /// <summary>
    /// 用于 <see cref="ArgumentOutOfRangeException(string?, object?, string?)"/> 的错误消息
    /// <para>https://github.com/dotnet/runtime/blob/v8.0.0-rc.2.23479.6/src/libraries/System.Private.CoreLib/src/Resources/Strings.resx#L205</para>
    /// </summary>
    public const string Arg_ArgumentOutOfRangeException__ =
"""
Specified argument was out of the range of valid values. (Parameter '{0}')
Actual value was {1}.
""";

    /// <summary>
    /// 用于 <see cref="ArgumentOutOfRangeException(string?, object?, string?)"/> 的错误消息
    /// <para>https://github.com/dotnet/runtime/blob/v8.0.0-rc.2.23479.6/src/libraries/System.Private.CoreLib/src/Resources/Strings.resx#L205</para>
    /// </summary>
    public const string Arg_ArgumentOutOfRangeException___ =
"""
{2} (Parameter '{0}')
Actual value was {1}.
""";
}