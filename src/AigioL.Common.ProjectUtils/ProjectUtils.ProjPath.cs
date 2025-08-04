using static System.Internals;

namespace System;

static partial class ProjectUtils
{
    /// <summary>
    /// 当前项目绝对路径
    /// </summary>
    public static string ProjPath => ProjPaths[0];

    /// <summary>
    /// 当前项目的顶级绝对路径（通常作为子模块返回仓库的项目路径）
    /// </summary>
    public static string ROOT_ProjPath => ProjPaths[1];

    /// <summary>
    /// 获取当前项目绝对路径(.sln/slnx 文件所在目录)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetProjectPath(string? path = null)
    {
        path ??=
#if NET46_OR_GREATER || NETCOREAPP
        AppContext.BaseDirectory;
#else
        AppDomain.CurrentDomain.BaseDirectory;
#endif
        try
        {
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable RS1035 // 不要使用禁用于分析器的 API
#pragma warning disable SA1003 // Symbols should be spaced correctly
#pragma warning disable SA1008 // Opening parenthesis should be spaced correctly
#pragma warning disable SA1110 // Opening parenthesis or bracket should be on declaration line
            if (!EnumerateSlnFiles(path).Any())
#pragma warning restore SA1003 // Symbols should be spaced correctly
            {
                var parent = Directory.GetParent(path);
                if (parent == null)
                    return string.Empty;
                return GetProjectPath(parent.FullName);
            }
#pragma warning restore SA1110 // Opening parenthesis or bracket should be on declaration line
#pragma warning restore SA1008 // Opening parenthesis should be spaced correctly
#pragma warning restore RS1035 // 不要使用禁用于分析器的 API
#pragma warning restore IDE0079 // 请删除不必要的忽略
        }
        catch
        {
            return string.Empty;
        }
        return path;
    }
}

file static class Internals
{
    internal static string[] ProjPaths => field ??= GetProjPath();

    internal static string[] GetProjPath()
    {
        var lProjPath = GetProjectPath();
        var lROOT_ProjPath = lProjPath;
#if (NETFRAMEWORK && NET40_OR_GREATER) || !NETFRAMEWORK
        if (!string.IsNullOrWhiteSpace(lROOT_ProjPath))
        {
            var mROOT_ProjPath = lROOT_ProjPath;
            var mROOT_ProjPath2 = mROOT_ProjPath;
            while (true)
            {
                mROOT_ProjPath = Path.Combine(mROOT_ProjPath, "..");
                mROOT_ProjPath = GetProjectPath(mROOT_ProjPath);
                if (string.IsNullOrWhiteSpace(mROOT_ProjPath))
                {
                    lROOT_ProjPath = mROOT_ProjPath2;
                    break;
                }
                mROOT_ProjPath2 = mROOT_ProjPath;
            }
        }
#endif
        return [lProjPath, lROOT_ProjPath];
    }

    internal static IEnumerable<string> EnumerateSlnFiles(string path)
    {
#if NET35
        return Directory.GetFiles(path, "*.sln").Concat(Directory.GetFiles(path, "*.slnx"));
#else
        return Directory.EnumerateFiles(path, "*.sln").Concat(Directory.EnumerateFiles(path, "*.slnx"));
#endif
    }
}