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
}
