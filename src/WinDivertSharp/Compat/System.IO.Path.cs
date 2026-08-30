#if NET35
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.IO;

internal static class PathCompat
{
    public static string Combine(string path1, string path2, string path3)
    {
        return Path.Combine(Path.Combine(path1, path2), path3);
    }
}
#endif