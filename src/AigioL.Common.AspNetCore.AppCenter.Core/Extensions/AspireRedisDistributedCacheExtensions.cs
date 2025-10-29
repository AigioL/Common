using Aspire.StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;
using StackExchange.Redis.Maintenance;
using StackExchange.Redis.Profiling;
using System.Net;
using System.Reflection;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.Hosting;

public static partial class AspireRedisDistributedCacheExtensions
{
    /// <summary>
    /// 添加由 Redis 实现的分布式缓存
    /// </summary>
    public static void AddRedisDistributedCacheV2(
        this IHostApplicationBuilder builder,
        string connectionName,
        string instanceName = "AigioL_Common_DistributedCache",
        Action<StackExchangeRedisSettings>? configureSettings = null,
        Action<ConfigurationOptions>? configureOptions = null,
        bool addMemoryCache = true)
    {
        string? connectionString = null;

        // 调用 Aspire 的扩展方法，添加 Redis 客户端和分布式缓存服务
        // https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/distributed?view=aspnetcore-10.0
        builder.AddRedisDistributedCache(connectionName, ConfigureSettings, configureOptions);

        // 通过委托获取连接字符串
        void ConfigureSettings(StackExchangeRedisSettings settings)
        {
            configureSettings?.Invoke(settings);
            connectionString = settings.ConnectionString;
        }

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.Services.TryAddSingleton<IConnectionMultiplexer>(new LazyConnectionMultiplexer(connectionString));
        }

        builder.Services.Configure<RedisCacheOptions>(options =>
        {
            options.InstanceName = instanceName;
        });

        if (addMemoryCache)
        {
            // 添加内存中缓存 https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/memory?view=aspnetcore-10.0
            builder.Services.AddMemoryCache();
        }
    }
}


file sealed class LazyConnectionMultiplexer(string redisConnStr) : IConnectionMultiplexer
{
    string IConnectionMultiplexer.ToString() => @this.Value.ToString()!;

    readonly Lazy<IConnectionMultiplexer> @this = new(() =>
    {
        try
        {
            // https://docs.redis.com/latest/rs/references/client_references/client_csharp/#aspnet-core
            var multiplexer = ConnectionMultiplexer.Connect(redisConnStr);
            return multiplexer;
        }
        catch
        {
            try
            {
                if (Assembly.GetEntryAssembly()?.GetName()?.Name == "GetDocument.Insider")
                {
                    // 生成文档时候不需要连接 Redis
                    return null!;
                }
            }
            catch
            {
            }
            throw;
        }
    }, true);

    public string ClientName => @this.Value.ClientName;

    public string Configuration => @this.Value.Configuration;

    public int TimeoutMilliseconds => @this.Value.TimeoutMilliseconds;

    public long OperationCount => @this.Value.OperationCount;

    [Obsolete]
    public bool PreserveAsyncOrder { get => @this.Value.PreserveAsyncOrder; set => @this.Value.PreserveAsyncOrder = value; }

    public bool IsConnected => @this.Value.IsConnected;

    public bool IsConnecting => @this.Value.IsConnecting;

    [Obsolete]
    public bool IncludeDetailInExceptions { get => @this.Value.IncludeDetailInExceptions; set => @this.Value.IncludeDetailInExceptions = value; }

    public int StormLogThreshold { get => @this.Value.StormLogThreshold; set => @this.Value.StormLogThreshold = value; }

    public event EventHandler<RedisErrorEventArgs> ErrorMessage
    {
        add
        {
            @this.Value.ErrorMessage += value;
        }

        remove
        {
            @this.Value.ErrorMessage -= value;
        }
    }

    public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed
    {
        add
        {
            @this.Value.ConnectionFailed += value;
        }

        remove
        {
            @this.Value.ConnectionFailed -= value;
        }
    }

    public event EventHandler<InternalErrorEventArgs> InternalError
    {
        add
        {
            @this.Value.InternalError += value;
        }

        remove
        {
            @this.Value.InternalError -= value;
        }
    }

    public event EventHandler<ConnectionFailedEventArgs> ConnectionRestored
    {
        add
        {
            @this.Value.ConnectionRestored += value;
        }

        remove
        {
            @this.Value.ConnectionRestored -= value;
        }
    }

    public event EventHandler<EndPointEventArgs> ConfigurationChanged
    {
        add
        {
            @this.Value.ConfigurationChanged += value;
        }

        remove
        {
            @this.Value.ConfigurationChanged -= value;
        }
    }

    public event EventHandler<EndPointEventArgs> ConfigurationChangedBroadcast
    {
        add
        {
            @this.Value.ConfigurationChangedBroadcast += value;
        }

        remove
        {
            @this.Value.ConfigurationChangedBroadcast -= value;
        }
    }

    public event EventHandler<ServerMaintenanceEvent> ServerMaintenanceEvent
    {
        add
        {
            @this.Value.ServerMaintenanceEvent += value;
        }

        remove
        {
            @this.Value.ServerMaintenanceEvent -= value;
        }
    }

    public event EventHandler<HashSlotMovedEventArgs> HashSlotMoved
    {
        add
        {
            @this.Value.HashSlotMoved += value;
        }

        remove
        {
            @this.Value.HashSlotMoved -= value;
        }
    }

    public void AddLibraryNameSuffix(string suffix)
    {
        @this.Value.AddLibraryNameSuffix(suffix);
    }

    public void Close(bool allowCommandsToComplete = true)
    {
        @this.Value.Close(allowCommandsToComplete);
    }

    public Task CloseAsync(bool allowCommandsToComplete = true)
    {
        return @this.Value.CloseAsync(allowCommandsToComplete);
    }

    public bool Configure(TextWriter? log = null)
    {
        return @this.Value.Configure(log);
    }

    public Task<bool> ConfigureAsync(TextWriter? log = null)
    {
        return @this.Value.ConfigureAsync(log);
    }

    public void Dispose()
    {
        @this.Value.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return @this.Value.DisposeAsync();
    }

    public void ExportConfiguration(Stream destination, StackExchange.Redis.ExportOptions options = (StackExchange.Redis.ExportOptions)(-1))
    {
        @this.Value.ExportConfiguration(destination, options);
    }

    public ServerCounters GetCounters()
    {
        return @this.Value.GetCounters();
    }

    public IDatabase GetDatabase(int db = -1, object? asyncState = null)
    {
        return @this.Value.GetDatabase(db, asyncState);
    }

    public EndPoint[] GetEndPoints(bool configuredOnly = false)
    {
        return @this.Value.GetEndPoints(configuredOnly);
    }

    public int GetHashSlot(RedisKey key)
    {
        return @this.Value.GetHashSlot(key);
    }

    public IServer GetServer(string host, int port, object? asyncState = null)
    {
        return @this.Value.GetServer(host, port, asyncState);
    }

    public IServer GetServer(string hostAndPort, object? asyncState = null)
    {
        return @this.Value.GetServer(hostAndPort, asyncState);
    }

    public IServer GetServer(IPAddress host, int port)
    {
        return @this.Value.GetServer(host, port);
    }

    public IServer GetServer(EndPoint endpoint, object? asyncState = null)
    {
        return @this.Value.GetServer(endpoint, asyncState);
    }

    public IServer[] GetServers()
    {
        return @this.Value.GetServers();
    }

    public string GetStatus()
    {
        return @this.Value.GetStatus();
    }

    public void GetStatus(TextWriter log)
    {
        @this.Value.GetStatus(log);
    }

    public string? GetStormLog()
    {
        return @this.Value.GetStormLog();
    }

    public ISubscriber GetSubscriber(object? asyncState = null)
    {
        return @this.Value.GetSubscriber(asyncState);
    }

    public int HashSlot(RedisKey key)
    {
        return @this.Value.HashSlot(key);
    }

    public long PublishReconfigure(CommandFlags flags = CommandFlags.None)
    {
        return @this.Value.PublishReconfigure(flags);
    }

    public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None)
    {
        return @this.Value.PublishReconfigureAsync(flags);
    }

    public void RegisterProfiler(Func<ProfilingSession?> profilingSessionProvider)
    {
        @this.Value.RegisterProfiler(profilingSessionProvider);
    }

    public void ResetStormLog()
    {
        @this.Value.ResetStormLog();
    }

    public void Wait(Task task)
    {
        @this.Value.Wait(task);
    }

    public T Wait<T>(Task<T> task)
    {
        return @this.Value.Wait(task);
    }

    public void WaitAll(params Task[] tasks)
    {
        @this.Value.WaitAll(tasks);
    }

    public IServer GetServer(RedisKey key, object? asyncState, CommandFlags flags)
    {
        return @this.Value.GetServer(key, asyncState, flags);
    }
}