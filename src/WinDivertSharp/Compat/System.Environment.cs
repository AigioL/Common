#if NET35
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System;

internal static class EnvironmentCompat
{
    public static bool Is64BitProcess => IntPtr.Size == 8;
}
#endif