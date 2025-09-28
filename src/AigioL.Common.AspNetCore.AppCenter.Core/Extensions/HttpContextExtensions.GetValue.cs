using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

static partial class HttpContextExtensions
{
    /// <summary>
    /// 尝试从请求 Query 中获取值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetValueByQuery(this HttpContext context, string key, [NotNullWhen(true)] out string? value)
    {
        StringValues val = context.Request.Query[key];
        bool isNullOrEmpty = StringValues.IsNullOrEmpty(val);
        value = val;
        return !isNullOrEmpty;
    }

    /// <summary>
    /// 尝试从请求 Session 中获取值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="remove"></param>
    /// <returns></returns>
    public static bool TryGetValueBySession(this HttpContext context, string key, [NotNullWhen(true)] out string? value, bool remove = false)
    {
        value = context.Session.GetString(key);
        if (remove)
        {
            context.Session.Remove(key);
        }
        bool isNullOrEmpty = string.IsNullOrEmpty(value);
        return !isNullOrEmpty;
    }

    /// <summary>
    /// 从请求 Query 或 Session 中通过键取值
    /// </summary>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string? GetQueryOrSessionValue(this HttpContext context, string key)
    {
        if (!TryGetValueByQuery(context, key, out var value))
        {
            if (!TryGetValueBySession(context, key, out value))
            {
            }
        }
        return value;
    }
}
