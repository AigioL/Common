#if NETSTANDARD || NETFRAMEWORK
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Runtime.InteropServices;

internal static class NativeLibrary
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern nint LoadLibrary(string lpFileName);

    public static nint Load(string libraryPath) => LoadLibrary(libraryPath);
}
#endif