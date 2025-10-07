using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AigioL.Common.UnitTest;

sealed class Program
{
    static IServiceProvider serviceProvider = null!;

    internal static IServiceProvider Services => serviceProvider;

    static string assemblyLocation = null!;

    internal static string AssemblyLocation => assemblyLocation;

    internal static string currentVersion = null!;

    internal static string currentBuild = null!;

    /// <summary>
    /// 伪入口点，在测试项目中实现类似控制台程序的入口点行为，非静态函数避免被识别为入口点冲突
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    internal int Main(string[] args)
    {
#pragma warning disable IL3000 // Avoid accessing Assembly file path when publishing as a single file
        assemblyLocation = typeof(Program).Assembly.Location;
#pragma warning restore IL3000 // Avoid accessing Assembly file path when publishing as a single file
        if (string.IsNullOrWhiteSpace(assemblyLocation))
        {
#pragma warning disable CA2208 // 正确实例化参数异常
            throw new ArgumentNullException(nameof(assemblyLocation), "程序集位置不可用");
#pragma warning restore CA2208 // 正确实例化参数异常
        }

        LogInit.InitLog("AigioL.Common.UnitTest");

        const string pkgName = "com.github.aigiol.common.unittest";
        var appDataDirectory = Path.Combine(assemblyLocation, "..", "AppData");

        currentVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version;
        currentBuild = Version.Parse(currentVersion).Build.ToString();

        ServiceCollection services = new();
        services.AddEssential(
            pkgName, default, currentVersion,
            currentBuild, appDataDirectory);

        serviceProvider = services.BuildServiceProvider();

        return 0;
    }
}
