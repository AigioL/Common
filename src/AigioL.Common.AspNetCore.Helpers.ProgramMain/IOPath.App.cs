using AigioL.Common.AspNetCore.Helpers.ProgramMain;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.IO;

static partial class IOPath
{
    /// <summary>
    /// 获取应用程序数据的位置
    /// </summary>
    public static string AppDataDirectory
    {
        get
        {
            if (field == null)
            {
                var appDataDirectory = C914ceb2.GetAppDataDirectory();
                ArgumentNullException.ThrowIfNull(appDataDirectory);
                field = appDataDirectory;
            }
            return field;
        }
    }

    /// <summary>
    /// 获取缓存数据的位置
    /// </summary>
    public static string CacheDirectory
    {
        get
        {
            if (field == null)
            {
                var cacheDirectory = C914ceb2.GetCacheDirectory();
                ArgumentNullException.ThrowIfNull(cacheDirectory);
                field = cacheDirectory;
            }
            return field;
        }
    }
}

file static class C914ceb2
{
    internal static string GetAppDataDirectory()
    {
        var environment = ProgramHelper.App.Environment;
        var value = Path.Combine(environment.ContentRootPath, "AppData");
        return value;
    }

    internal static string GetCacheDirectory()
    {
        var environment = ProgramHelper.App.Environment;
        var value = Path.Combine(environment.ContentRootPath, "Cache");
        return value;
    }
}