using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AigioL.Common.AspNetCore.Helpers.ProgramMain;

static partial class ProgramHelper
{
    const string VersionZero = "0.0.0.0";

    /// <summary>
    /// 当前 Program 的项目名称
    /// </summary>
    public static string ProjectName { get; private set; } = string.Empty;

    /// <summary>
    /// 当前 Program 版本号
    /// </summary>
    public static string Version { get; private set; } = VersionZero;

    static void CalcVersion(Assembly? assembly = null)
    {
        if (assembly == null)
        {
            try
            {
                assembly = Assembly.GetCallingAssembly();
            }
            catch
            {
            }
        }
        if (assembly == null)
        {
            assembly = typeof(ProgramHelper).Assembly;
        }

        var version = assembly.GetName().Version?.ToString();
        if (string.IsNullOrWhiteSpace(version) || !global::System.Version.TryParse(version, out var _))
        {
            version = VersionZero;
        }
        Version = version;
    }

    public static void ConsoleWriteInfo(string? projectName = default)
    {
        if (!string.IsNullOrWhiteSpace(projectName))
            ProjectName = projectName;

        #region 项目代号和版本信息

        if (!string.IsNullOrWhiteSpace(projectName))
        {
            Console.Write("Project ");
            ConsoleWriteTrimStart(projectName, "Project");
            const string version_f = $" [{nameof(Version)} ";
            Console.Write(version_f);

            if (string.IsNullOrWhiteSpace(Version) || Version == VersionZero)
            {
                Assembly? callingAssembly = default;
                try
                {
                    callingAssembly = Assembly.GetCallingAssembly();
                }
                catch
                {
                }
                CalcVersion(callingAssembly);
            }

            Console.Write(Version);
            Console.Write(" / Runtime ");
            Console.Write(Environment.Version);
            Console.Write(']');
            Console.Write('\n');
            Console.Write('\n');
        }

        #endregion

        #region 当前运行的计算机 CPU 显示名称

        //if (!string.IsNullOrEmpty(CentralProcessorName))
        //{
        //    Console.Write("CentralProcessorName: ");
        //    Console.Write(CentralProcessorName);
        //    Console.Write(" x");
        //    Console.Write(Environment.ProcessorCount);
        //    Console.Write('\n');
        //}

        #endregion

        #region 本地时间与当前系统设置区域

        Console.Write("LocalTime: ");
        Console.Write(DateTimeOffset.Now.ToLocalTime());
        Console.Write('\n');

        Console.Write("CurrentCulture: ");
        Console.Write(CultureInfo.CurrentCulture.Name);
        Console.Write(' ');
        Console.Write(CultureInfo.CurrentCulture.EnglishName);
        Console.Write('\n');

        #endregion

        #region ShowInfo

        Console.Write("BaseDirectory: ");
        Console.Write(AppContext.BaseDirectory);
        Console.Write('\n');

        Console.Write("OSArchitecture: ");
        Console.Write(RuntimeInformation.OSArchitecture);
        Console.Write('\n');

        Console.Write("ProcessArchitecture: ");
        Console.Write(RuntimeInformation.ProcessArchitecture);
        Console.Write('\n');

        Console.Write("ProcessId: ");
        Console.Write(Environment.ProcessId);
        Console.Write('\n');

        Console.Write("ProcessorCount: ");
        Console.Write(Environment.ProcessorCount);
        Console.Write('\n');

        Console.Write("CurrentManagedThreadId: ");
        Console.Write(Environment.CurrentManagedThreadId);
        Console.Write('\n');

        Console.Write("RuntimeVersion: ");
        Console.Write(Environment.Version);
        Console.Write('\n');

        Console.Write("OSVersion: ");
        Console.Write(Environment.OSVersion.Version);
        Console.Write('\n');

        Console.Write("OSVersionString: ");
        Console.Write(Environment.OSVersion.VersionString);
        Console.Write('\n');

        Console.Write("UserInteractive: ");
        Console.Write(Environment.UserInteractive);
        Console.Write('\n');

        Console.Write("MachineName: ");
        Console.Write(Environment.MachineName);
        Console.Write('\n');

        Console.Write("UserName: ");
        Console.Write(Environment.UserName);
        Console.Write('\n');

        Console.Write("UserDomainName: ");
        Console.Write(Environment.UserDomainName);
        Console.Write('\n');

        Console.Write("IsPrivilegedProcess: ");
        Console.Write(Environment.IsPrivilegedProcess);
        Console.Write('\n');

        Console.Write("Is64BitOperatingSystem: ");
        Console.Write(Environment.Is64BitOperatingSystem);
        Console.Write('\n');

        Console.Write("Is64BitProcess: ");
        Console.Write(Environment.Is64BitProcess);
        Console.Write('\n');

        Console.Write("SystemPageSize: ");
        Console.Write(Environment.SystemPageSize);
        Console.Write('\n');

        Console.Write("IsDynamicCodeCompiled: ");
        Console.Write(RuntimeFeature.IsDynamicCodeCompiled);
        Console.Write('\n');

        Console.Write("IsDynamicCodeSupported: ");
        Console.Write(RuntimeFeature.IsDynamicCodeSupported);
        Console.Write('\n');

        #endregion

        Console.Write('\n');
    }

    static void ConsoleWriteTrimStart(string s, string value)
    {
        if (s.StartsWith(value))
        {
            ReadOnlySpan<char> chars = s.AsSpan()[value.Length..];
            for (int i = 0; i < chars.Length; i++)
            {
                Console.Write(chars[i]);
            }
        }
        else
        {
            Console.Write(s);
        }
    }
}
