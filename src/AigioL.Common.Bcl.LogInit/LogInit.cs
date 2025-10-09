#if DEBUG || USE_LOGGING_CONSOLE
using Microsoft.Extensions.Logging.Console;
#endif
#if WINDOWS
using Microsoft.Extensions.Logging.EventLog;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;

namespace System;

/// <summary>
/// 日志初始化类，提供全局日志配置与初始化方法
/// </summary>
public static partial class LogInit
{
    /// <summary>
    /// 使用日志源名称初始化日志工厂与提供程序
    /// </summary>
    public static void InitLog(string? sourceName = null) => _8b7542f7.InitLog(sourceName);

    /// <summary>
    /// 设置或获取全局日志级别
    /// </summary>
    public static LogLevel LogLevel
    {
        get => _8b7542f7.GetLogLevel();
        set => _8b7542f7.SetLogLevel(value);
    }

#if WINDOWS
    /// <summary>
    /// 设置或获取 Windows 事件日志级别
    /// </summary>
    public static LogLevel? LogLevelEventLog
    {
        get => _8b7542f7.GetLogLevelEventLog();
        set => _8b7542f7.SetLogLevelEventLog(value);
    }
#endif
}

/// <summary>
/// <see cref="IOptionsMonitor{TOptions}"/> 的简单实现
/// </summary>
/// <typeparam name="TOptions"></typeparam>
file class SimpleOptionsMonitor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOptions> : IOptionsMonitor<TOptions>, IOptions<TOptions>
    where TOptions : class
{
    TOptions value;

    internal SimpleOptionsMonitor(TOptions value)
    {
        this.value = value;
    }

    public TOptions CurrentValue => value;

    public TOptions Value => value;

    public TOptions Get(string? name) => value;

    public void Set(TOptions value, string? name = null)
    {
        // 设置值触发变更事件
        this.value = value;
        OnChangeEvent?.Invoke(value, name);
    }

    event Action<TOptions, string?>? OnChangeEvent;

    public IDisposable? OnChange(Action<TOptions, string?> listener)
    {
        // 添加变更事件监听器
        OnChangeEvent += listener;
        return new AnonymousDisposable(() =>
        {
            // 当调用 Dispose 时，移除变更事件监听器
            OnChangeEvent -= listener;
        });
    }

    /// <summary>
    /// https://github.com/dotnet/reactive/blob/main/Rx.NET/Source/src/System.Reactive/Disposables/AnonymousDisposable.cs
    /// </summary>
    sealed class AnonymousDisposable : IDisposable
    {
        volatile Action? _dispose;

        /// <summary>
        /// Constructs a new disposable with the given action used for disposal.
        /// </summary>
        /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
        public AnonymousDisposable(Action dispose)
        {
            Debug.Assert(dispose != null);

            _dispose = dispose;
        }

        ///// <summary>
        ///// Gets a value that indicates whether the object is disposed.
        ///// </summary>
        //public bool IsDisposed => _dispose == null;

        /// <summary>
        /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }
}

file static partial class _8b7542f7
{
    static SimpleOptionsMonitor<LoggerFilterOptions>? filterOptionsMonitor;

    internal static void InitLog(string? sourceName)
    {
        LoggerFilterOptions filterOptions = new()
        {
            MinLevel = logLevelConst,
        };
        filterOptionsMonitor = new(filterOptions);
        ILoggerProvider[] providers = [
#if DEBUG || USE_LOGGING_CONSOLE
            AddConsole(),
#endif
#if WINDOWS
            AddEventLog(sourceName),
#endif
        ];
        var factory = new LoggerFactory(providers, filterOptionsMonitor);
        Log.ConfigureLoggerFactory(factory);
    }
}

file static partial class _8b7542f7 // 全局日志等级
{
    static LogLevel logLevelConst = LogLevel.
#if DEBUG
        Debug;
#else
        Information;
#endif

    internal static LogLevel GetLogLevel()
    {
        if (filterOptionsMonitor == null)
        {
            return logLevelConst;
        }
        else
        {
            return filterOptionsMonitor.Value.MinLevel;
        }
    }

    internal static void SetLogLevel(LogLevel value)
    {
        if (filterOptionsMonitor == null)
        {
            // 如果未初始化
            if (logLevelConst != value)
            {
                logLevelConst = value;
            }
        }
        else if (filterOptionsMonitor.Value.MinLevel != value)
        {
            filterOptionsMonitor.Value.MinLevel = value;
            filterOptionsMonitor.Set(filterOptionsMonitor.Value, null);
        }
    }
}

#if DEBUG || USE_LOGGING_CONSOLE
file static partial class _8b7542f7 // 控制台日志
{
    /// <summary>
    /// 添加控制台日志记录提供程序
    /// </summary>
    /// <returns></returns>
    internal static ILoggerProvider AddConsole()
    {
        // Release 模式下使用 WinExe，不显示控制台
        // 可以调用 Win32 函数的封装 ConsoleDisposable.ShowConsole() 方法显示控制台
        // 控制台日志提供程序的 Options 没有像 EventLog 提供程序一样能设置自定义 Filter
        // 如果要实现 Filter，需要写一个包装类，包装控制台提供程序实现 ILoggerProvider 与 ILogger 以重写 IsEnabled 函数
        ConsoleLoggerOptions options = new();
        SimpleOptionsMonitor<ConsoleLoggerOptions> optionsMonitor = new(options);
        ConsoleLoggerProvider provider = new(optionsMonitor);
        return provider;
    }
}
#endif

#if WINDOWS
file static partial class _8b7542f7 // Windows 事件日志
{
    static LogLevel? logLevelEventLog =
#if DEBUG
        LogLevel.None; // 调试模式下日志见控制台输出，不记录到事件日志中
#else
        LogLevel.Warning; // 发布模式下记录 Warning 及以上级别的日志到事件日志中
#endif

    internal static LogLevel? GetLogLevelEventLog() => logLevelEventLog;

    internal static void SetLogLevelEventLog(LogLevel? value)
    {
        if (logLevelEventLog != value)
        {
            logLevelEventLog = value;
        }
    }

    static string? TryGetProcessFileNameWithoutExtension()
    {
        try
        {
            var result = Path.GetFileNameWithoutExtension(Environment.ProcessPath);
            return result;
        }
        catch
        {
            return null;
        }
    }

    static bool FilterEventLog(string name, LogLevel level)
    {
        // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.EventLog/src/EventLogLogger.cs#L63

        var globalLevel = GetLogLevel();
        if (level >= globalLevel) // 日志等级大于等于全局配置
        {
            if (logLevelEventLog.HasValue)
            {
                if (level >= logLevelEventLog.Value) // 日志等级大于等于事件日志配置
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 添加事件日志记录提供程序
    /// <list type="bullet">
    /// <item>Win+R 打开运行输入 "%windir%\system32\compmgmt.msc /s" 启动计算机管理</item>
    /// <item>计算机管理 - 系统工具 - 事件查看器 - Windows 日志 - 应用程序</item>
    /// <item>筛选当前日志 - 事件来源 == <see cref="EventLogSettings.SourceName"/></item>
    /// </list>
    /// </summary>
    internal static ILoggerProvider AddEventLog(string? sourceName = null)
    {
        sourceName ??= TryGetProcessFileNameWithoutExtension();
        EventLogSettings settings = new()
        {
            SourceName = sourceName, // 必须设置值，否则 null 值会使用默认值为 ".NET Runtime"
            LogName = null, // null 值会使用默认值为 "Application"
            Filter = FilterEventLog,
        };
        EventLogLoggerProvider provider = new(settings);
        return provider;
    }
}
#endif