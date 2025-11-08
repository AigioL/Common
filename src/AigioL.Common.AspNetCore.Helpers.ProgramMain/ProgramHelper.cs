using AigioL.Common.EntityFrameworkCore.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NLog.Web;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json.Nodes;
using NLogLevel = NLog.LogLevel;

namespace AigioL.Common.AspNetCore.Helpers.ProgramMain;

public static partial class ProgramHelper
{
    static readonly WebApplication? app;

    public static WebApplication App
    {
        get
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app),
                    "WebApplication is not initialized. Please call Main method first.");
            }
            return app;
        }
    }

    public static IDisposable? Disposable => app;

    /// <summary>
    /// 适用于 ASP.NET Core 6.0+ 中新的最小托管模型的代码
    /// </summary>
    public static unsafe void M(
       string projectName,
       string[] args,
       delegate* managed<WebApplicationBuilder, void> configureServices = default,
       delegate* managed<WebApplication, void> configure = default,
       WebApplicationBuilder? builder = default,
       Assembly? callingAssembly = null,
       //Encoding? outputEncoding = null,
       long archiveAboveSize = archiveAboveSize,
       int maxArchiveFiles = maxArchiveFiles,
       int maxArchiveDays = maxArchiveDays)
    {
        SetProject(projectName);

        try
        {
            callingAssembly ??= Assembly.GetCallingAssembly();
        }
        catch
        {
        }

        var logger = LogManager.Setup()
                                .RegisterNLogWeb()
                                .LoadConfiguration(InitNLogConfig(archiveAboveSize, maxArchiveFiles, maxArchiveDays))
                                .GetCurrentClassLogger();
        bool isDevelopment =
#if DEBUG
            true;
#else
            false;
#endif
        try
        {
            //Console.OutputEncoding = outputEncoding ?? Encoding.Unicode; // 使用 UTF16 编码输出控制台文字
            CalcVersion(callingAssembly);

            ConsoleWriteInfo(projectName, isDevelopment);

            // https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-6
            logger.Debug("init main");
            builder ??= WebApplication.CreateSlimBuilder(args);
            isDevelopment = builder.Environment.IsDevelopment();
            builder.WebHost.ConfigureKestrel(static o =>
            {
                o.AddServerHeader = false;
            });
            builder.Host.UseNLog();
            if (configureServices != default)
            {
                configureServices(builder);
                configureServices = default;
            }
            var app = builder.Build();
            Log.ConfigureLoggerFactory(app.Services.GetRequiredService<ILoggerFactory>());
            //InitFileSystem(app.Environment);
            //Ioc.ConfigureServices(app.Services);
            if (configure != default)
            {
                configure(app);
                configure = default;
            }

            app.Run();
        }
        catch (Exception exception)
        {
            //NLog: catch setup errors
            logger.Error(exception, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }
    }

    /// <summary>
    /// 初始化 NLog 配置
    /// </summary>
    /// <returns></returns>
    static LoggingConfiguration InitNLogConfig(long archiveAboveSize, int maxArchiveFiles, int maxArchiveDays)
    {
        // https://github.com/NLog/NLog/wiki/Getting-started-with-ASP.NET-Core-6

        var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
        CreateDirectory(logsPath);

        InternalLogger.LogFile = $"logs{Path.DirectorySeparatorChar}internal-nlog.txt";
        InternalLogger.LogLevel =
#if DEBUG
            NLogLevel.Info;
#else
            NLogLevel.Error;
#endif
        // enable asp.net core layout renderers
        LogManager.Setup().SetupExtensions(s => s.RegisterAssembly("NLog.Web.AspNetCore"));

        var objConfig = new LoggingConfiguration();
        // File Target for all log messages with basic details
        var allfile = new FileTarget("allfile")
        {
            FileName = $"logs{Path.DirectorySeparatorChar}nlog-all-${{shortdate}}.log",
            Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}",
            ArchiveAboveSize = archiveAboveSize,
            MaxArchiveFiles = maxArchiveFiles,
            MaxArchiveDays = maxArchiveDays,
        };
        objConfig.AddTarget(allfile);
        // File Target for own log messages with extra web details using some ASP.NET core renderers
        var ownFile_web = new FileTarget("ownFile-web")
        {
            FileName = $"logs{Path.DirectorySeparatorChar}nlog-own-${{shortdate}}.log",
            Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}",
            ArchiveAboveSize = archiveAboveSize,
            MaxArchiveFiles = maxArchiveFiles,
            MaxArchiveDays = maxArchiveDays,
        };
        objConfig.AddTarget(ownFile_web);
        // Console Target for hosting lifetime messages to improve Docker / Visual Studio startup detection
        var lifetimeConsole = new ConsoleTarget("lifetimeConsole")
        {
            Layout = "${level:truncate=4:tolower=true}\\: ${logger}[0]${newline}      ${message}${exception:format=tostring}",
        };
        objConfig.AddTarget(lifetimeConsole);

        var ruleMinLevel =
#if DEBUG
            NLogLevel.Trace;
#else
            NLogLevel.Error;
#endif

        // All logs, including from Microsoft
        objConfig.AddRule(ruleMinLevel, NLogLevel.Fatal, allfile, "*");
        objConfig.AddRule(ruleMinLevel, NLogLevel.Fatal, ownFile_web, "*");

        foreach (var target in objConfig.AllTargets)
        {
            // Skip non-critical Microsoft logs and so log only own logs (BlackHole)
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, target, "Microsoft.*", true);
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, target, "System.Net.Http.*", true);
        }

        foreach (var target in new Target[] { lifetimeConsole, ownFile_web })
            // Output hosting lifetime messages to console target for faster startup detection
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, target, "Microsoft.Hosting.Lifetime", true);

        return objConfig;

        //        var xmlConfigStr =
        //          "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
        //          "<nlog xmlns=\"http://www.nlog-project.org/schemas/NLog.xsd\"" +
        //          "      xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
        //          "      autoReload=\"true\"" +
        //          "      internalLogLevel=\"" +
        //#if DEBUG
        //          "Info"
        //#else
        //          "Error"
        //#endif
        //          + "\"" +
        //          "      internalLogFile=\"logs" + Path.DirectorySeparatorChar + "internal-nlog.txt\">" +
        //          // enable asp.net core layout renderers
        //          "  <extensions>" +
        //          "    <add assembly=\"NLog.Web.AspNetCore\"/>" +
        //          "  </extensions>" +
        //          // the targets to write to
        //          "  <targets>" +
        //          // write logs to file
        //          "    <target xsi:type=\"File\" name=\"allfile\" fileName=\"logs" + Path.DirectorySeparatorChar + "nlog-all-${shortdate}.log\"" +
        //          "            layout=\"${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}\"/>" +
        //          // another file log, only own logs. Uses some ASP.NET core renderers
        //          "    <target xsi:type=\"File\" name=\"ownFile-web\" fileName=\"logs" + Path.DirectorySeparatorChar + "nlog-own-${shortdate}.log\"" +
        //          "            layout=\"${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}\"/>" +
        //          // Console Target for hosting lifetime messages to improve Docker / Visual Studio startup detection
        //          "    <target xsi:type=\"Console\" name=\"lifetimeConsole\" layout=\"${level:truncate=4:tolower=true}\\: ${logger}[0]${newline}      ${message}${exception:format=tostring}\" />" +
        //          "  </targets>" +
        //          // rules to map from logger name to target
        //          "  <rules>" +
        //          // All logs, including from Microsoft
        //          "    <logger name=\"*\" minlevel=\"" +
        //#if DEBUG
        //          "Trace"
        //#else
        //          "Error"
        //#endif
        //          + "\" writeTo=\"allfile\"/>" +
        //          // Output hosting lifetime messages to console target for faster startup detection
        //          "    <logger name=\"Microsoft.Hosting.Lifetime\" minlevel=\"Info\" writeTo=\"lifetimeConsole, ownFile-web\" final=\"true\" />" +
        //          // Skip non-critical Microsoft logs and so log only own logs
        //          "    <logger name=\"Microsoft.*\" maxLevel=\"Info\" final=\"true\"/>" +
        //          "    <logger name=\"System.Net.Http.*\" maxlevel=\"Info\" final=\"true\" />" +
        //          // BlackHole without writeTo
        //          "    <logger name=\"*\" minlevel=\"" +
        //#if DEBUG
        //          "Trace"
        //#else
        //          "Error"
        //#endif
        //          + "\" writeTo=\"ownFile-web\"/>" +
        //          "  </rules>" +
        //          "</nlog>";

        //        var xmlConfig = XmlLoggingConfiguration.CreateFromXmlString(xmlConfigStr);

        //        return xmlConfig;
    }

    #region https://github.com/NLog/NLog/wiki/File-target

    /// <summary>
    /// 日志文件自动存档的字节大小
    /// </summary>
    const long archiveAboveSize = 10485760;

    /// <summary>
    /// 应保留的最大存档文件数。如果值小于或等于 0，则不会删除旧文件
    /// </summary>
    const int maxArchiveFiles = 10;

    /// <summary>
    /// 应保留的存档文件的最长期限。当 archiveNumbering 无效时。如果值小于或等于 0，则不会删除旧文件
    /// </summary>
    const int maxArchiveDays = 31;

    #endregion https://github.com/NLog/NLog/wiki/File-target

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [LibraryImport("libc", EntryPoint = "chmod", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [SupportedOSPlatform("FreeBSD")]
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("MacOS")]
    private static partial int Chmod(string path, int mode);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void CreateDirectory(string dirPath)
    {
        try
        {
            // 如果路径存在且是文件，则删除它
            if (File.Exists(dirPath))
            {
                File.Delete(dirPath);
            }
        }
        catch (Exception)
        {
        }

        var dirInfo = Directory.CreateDirectory(dirPath);
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
        {
            try
            {
                const UnixFileMode mode =
                    UnixFileMode.UserRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.GroupRead |
                    UnixFileMode.GroupWrite |
                    UnixFileMode.OtherRead |
                    UnixFileMode.OtherWrite;
                Chmod(dirInfo.FullName, unchecked((int)mode));
            }
            catch (Exception)
            {
            }
        }
    }

    /// <summary>
    /// 通过 .NET Aspire 资源名获取数据库连接字符串
    /// </summary>
    /// <param name="connectionName"></param>
    /// <param name="appHostUserSecretsId"></param>
    /// <param name="hostPortUsername"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetConnectionString(
        string connectionName,
#if DEBUG
        string appHostUserSecretsId,
        string? hostPortUsername,
#endif
        WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName);
#if DEBUG
        if (string.IsNullOrWhiteSpace(connectionString) && OperatingSystem.IsWindows())
        {
            JsonNode? postgres_password = null;
            var secrets_path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                $@"Microsoft\UserSecrets\{appHostUserSecretsId}\secrets.json");
            var secrets_u8 = File.ReadAllText(secrets_path);
            var secrets_obj = JsonNode.Parse(secrets_u8)?.AsObject();
            secrets_obj?.TryGetPropertyValue("Parameters:postgres-password", out postgres_password);
            if (postgres_password != null)
            {
                hostPortUsername ??= "Host=localhost;Port=5432;Username=postgres";
                connectionString = $"{hostPortUsername.AsSpan().TrimEnd(';')};Password={postgres_password.GetValue<string>()};Database={connectionName}";
            }
        }
#endif
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionString}' not found.");
        }
        return connectionString;
    }

    public interface IDbContext
    {
        DbContext GetDbContext();
    }
}
