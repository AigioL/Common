using System.Reflection;

namespace AigioL.Common.BuildTools;

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
            ConsoleWriteTrimStart(projectName, "Project");
            const string version_f = $" Build Tools [{nameof(Version)} ";
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
