using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.EntityFrameworkCore.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Web;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json.Nodes;
using NLogLevel = NLog.LogLevel;

namespace AigioL.Common.AspNetCore.Helpers.ProgramMain;

public static partial class ProgramHelper
{
    static bool isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
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

    static string GetProjectNameByProcessPath()
    {
        var processPath = Environment.ProcessPath;
        ArgumentNullException.ThrowIfNull(processPath);
        var chars = processPath.AsSpan();
        var index = chars.LastIndexOf(Path.DirectorySeparatorChar);
        if (index > 0)
        {
            chars = chars[(index + 1)..];
        }
        if (chars.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) ||
            chars.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
        {
            chars = chars[..^4];
        }

        var r = chars.ToString();
        return r;
    }

    /// <summary>
    /// 适用于 ASP.NET Core 6.0+ 中新的最小托管模型的代码
    /// </summary>
    public static unsafe void M(
       string? projectName,
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
        projectName ??= GetProjectNameByProcessPath();
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

            if (isContainer)
            {
                builder.Configuration.AddJsonFile("config/appsettings.k8s.json", optional: true, reloadOnChange: true);
            }

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
    /// 初始化 NLog 配置：支持本地文件 + K8S JSON 流
    /// </summary>
    static LoggingConfiguration InitNLogConfig(long archiveAboveSize, int maxArchiveFiles, int maxArchiveDays)
    {
        // 基础设置
        LogManager.Setup().SetupExtensions(s => s.RegisterAssembly("NLog.Web.AspNetCore"));
        var objConfig = new LoggingConfiguration();

        // 内部日志设置
        InternalLogger.LogFile = Path.Combine(AppContext.BaseDirectory, "logs", "internal-nlog.txt");
        InternalLogger.LogLevel =
#if DEBUG
        NLogLevel.Info;
#else
        NLogLevel.Error;
#endif

        if (isContainer)
        {
            // 【K8S 模式】使用 JSON 控制台输出 
            var k8sConsole = new ConsoleTarget("k8sConsole")
            {
                // 使用 JsonLayout，PLG 采集后可以直接按字段查询
                Layout = new JsonLayout
                {
                    Attributes =
                    {
                        new JsonAttribute("time", "${longdate}"),
                        new JsonAttribute("level", "${level:upperCase=true}"),
                        new JsonAttribute("logger", "${logger}"),
                        new JsonAttribute("message", "${message}"),
                        new JsonAttribute("exception", "${exception:format=tostring}"),
                        // 微服务链路追踪 ID
                        new JsonAttribute("traceId", "${aspnet-traceidentifier}"),
                        new JsonAttribute("url", "${aspnet-request-url}"),
                        new JsonAttribute("action", "${aspnet-mvc-action}")
                    }
                }
            };
            objConfig.AddTarget(k8sConsole);

            // K8S 规则：仅输出核心业务日志到控制台
            var minLevel =
#if DEBUG
                NLogLevel.Trace;
#else
            NLogLevel.Info; // 生产环境建议 Info
#endif
            objConfig.AddRule(minLevel, NLogLevel.Fatal, k8sConsole);
        }
        else
        {
            // 【本地模式】文件日志逻辑
            var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
            if (!Directory.Exists(logsPath)) Directory.CreateDirectory(logsPath);

            // 1. allfile
            var allfile = new FileTarget("allfile")
            {
                FileName = Path.Combine(logsPath, "nlog-all-${shortdate}.log"),
                Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}",
                ArchiveAboveSize = archiveAboveSize,
                MaxArchiveFiles = maxArchiveFiles,
                MaxArchiveDays = maxArchiveDays,
            };

            // 2. ownFile-web
            var ownFile_web = new FileTarget("ownFile-web")
            {
                FileName = Path.Combine(logsPath, "nlog-own-${shortdate}.log"),
                Layout = "${longdate}|${event-properties:item=EventId_Id}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}",
                ArchiveAboveSize = archiveAboveSize,
                MaxArchiveFiles = maxArchiveFiles,
                MaxArchiveDays = maxArchiveDays,
            };

            // 3. 方便 VS 查看的彩色控制台
            var lifetimeConsole = new ConsoleTarget("lifetimeConsole")
            {
                Layout = "${level:truncate=4:tolower=true}\\: ${logger}[0]${newline}      ${message}${exception:format=tostring}",
            };

            objConfig.AddTarget(allfile);
            objConfig.AddTarget(ownFile_web);
            objConfig.AddTarget(lifetimeConsole);

            // 本地规则
            objConfig.AddRule(NLogLevel.Trace, NLogLevel.Fatal, allfile);
            objConfig.AddRule(NLogLevel.Trace, NLogLevel.Fatal, ownFile_web);
            objConfig.AddRule(NLogLevel.Info, NLogLevel.Fatal, lifetimeConsole);
        }

        // 通用黑洞规则：屏蔽冗余的微软系统日志
        objConfig.AddRule(NLogLevel.Trace, NLogLevel.Info, new NullTarget(), "Microsoft.*", true);
        objConfig.AddRule(NLogLevel.Trace, NLogLevel.Info, new NullTarget(), "System.Net.Http.*", true);

        return objConfig;
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
        //IDbContext.ConnectionString = connectionString;
        return connectionString;
    }

    public interface IDbContext
    {
        DbContext GetDbContext();

        //static string? ConnectionString { get; internal set; }
    }
}
