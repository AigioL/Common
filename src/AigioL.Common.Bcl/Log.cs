using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace System;

/// <summary>
/// 日志
/// <para>编译时日志记录源生成：https://learn.microsoft.com/zh-cn/dotnet/core/extensions/logger-message-generator</para>
/// </summary>
public static partial class Log
{
    /// <inheritdoc cref="ILoggerFactory"/>
    public static ILoggerFactory Factory // 不可更改此属性类型！
    {
        get
        {
            try
            {
                var loggerFactory = _b7722e25.GetLoggerFactory();
                return loggerFactory;
            }
            catch (ObjectDisposedException)
            {
                return new EmptyLoggerFactory();
            }
        }
    }

    /// <inheritdoc cref="ILoggerFactory.CreateLogger(string)"/>
    public static ILogger CreateLogger(string tag)
    {
        try
        {
            var logger = Factory.CreateLogger(tag);
            return logger;
        }
        catch (ObjectDisposedException)
        {
            return new EmptyLogger();
        }
    }

    /// <inheritdoc cref="LoggerFactoryExtensions.CreateLogger{T}(ILoggerFactory)"/>
    public static ILogger CreateLogger<T>()
    {
        try
        {
            var logger = Factory.CreateLogger<T>();
            return logger;
        }
        catch (ObjectDisposedException)
        {
            return new EmptyLogger();
        }
    }

    /// <summary>
    /// 初始化共享的 <see cref="ILoggerFactory"/> 实例，初始化只能调用一次
    /// </summary>
    /// <param name="loggerFactory">要使用的输入 <see cref="ILoggerFactory"/> 实例</param>
    /// <exception cref="ArgumentNullException">如果 <paramref name="loggerFactory"/> 为 <see langword="null"/> 则抛出异常</exception>
    public static void ConfigureLoggerFactory(ILoggerFactory loggerFactory) => _b7722e25.ConfigureLoggerFactory(loggerFactory);
}

partial class Log // Empty
{
    readonly struct EmptyLogger : ILogger
    {
        IDisposable? ILogger.BeginScope<TState>(TState state)
        {
            return null;
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    readonly struct EmptyLoggerFactory : ILoggerFactory
    {
        void ILoggerFactory.AddProvider(ILoggerProvider provider)
        {
        }

        ILogger ILoggerFactory.CreateLogger(string categoryName)
        {
            return new EmptyLogger();
        }

        void IDisposable.Dispose()
        {
        }
    }
}

static partial class _b7722e25
{
    static ILoggerFactory? loggerFactory;

    internal static ILoggerFactory GetLoggerFactory()
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        return loggerFactory;
    }

    internal static void ConfigureLoggerFactory(ILoggerFactory loggerFactory)
    {
        // 参考 https://github.com/CommunityToolkit/dotnet/blob/main/src/CommunityToolkit.Mvvm/DependencyInjection/Ioc.cs#L135

        ArgumentNullException.ThrowIfNull(loggerFactory);

        ILoggerFactory? oldServices = Interlocked.CompareExchange(ref _b7722e25.loggerFactory, loggerFactory, null);

        if (oldServices is not null)
        {
            ThrowInvalidOperationExceptionForRepeatedConfiguration();
        }
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> when a configuration is attempted more than once.
    /// </summary>
    [DoesNotReturn]
    static void ThrowInvalidOperationExceptionForRepeatedConfiguration()
    {
        throw new InvalidOperationException("The default logger factory has already been configured.");
    }
}