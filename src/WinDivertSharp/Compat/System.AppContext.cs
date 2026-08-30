#if NET45 || NET40 || NET35
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

internal static class AppContext
{
    public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
}
#endif