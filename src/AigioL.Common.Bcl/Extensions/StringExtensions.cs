using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

public static partial class StringExtensions
{
    /// <summary>
    /// https://
    /// </summary>
    public const string Prefix_HTTPS = "https://";

    /// <summary>
    /// http://
    /// </summary>
    public const string Prefix_HTTP = "http://";

    /// <summary>
    /// 判断字符串是否为 HttpUrl
    /// </summary>
    /// <param name="str"></param>
    /// <param name="httpsOnly">是否仅Https</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHttpUrl([NotNullWhen(true)] this string? str, bool httpsOnly = false) => str != null &&
        (str.StartsWith(Prefix_HTTPS, StringComparison.OrdinalIgnoreCase) ||
              (!httpsOnly && str.StartsWith(Prefix_HTTP, StringComparison.InvariantCultureIgnoreCase)));

    /// <summary>
    /// 判断字符串是否为邮箱地址
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmail(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        // https://github.com/dotnet/runtime/blob/v10.0.0-rc.1.25451.107/src/libraries/System.ComponentModel.Annotations/src/System/ComponentModel/DataAnnotations/EmailAddressAttribute.cs#L18

        if (str.AsSpan().ContainsAny('\r', '\n'))
        {
            return false;
        }

        // only return true if there is only 1 '@' character
        // and it is neither the first nor the last character
        int index = str.IndexOf('@');

        return
            index > 0 &&
            index != str.Length - 1 &&
            index == str.LastIndexOf('@');
    }

    /// <inheritdoc cref="string.Format(string, object?[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Format(this string format, params object?[] args)
    {
        try
        {
            return string.Format(format, args);
        }
        catch
        {
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return string.Join(' ', new[] { format }.Concat(args));
#else
            return string.Join(" ", new[] { format }.Concat(args));
#endif
        }
    }
}
