using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

/// <summary>
/// 作业计划（JobScheduler）服务基类
/// </summary>
public abstract partial class JobService(
    ILogger logger,
    IJobDbContext dbContext,
    IFeishuApiClient feishuApiClient)
{
    /// <inheritdoc cref="ILogger"/>
    protected readonly ILogger logger = logger;

    /// <inheritdoc cref="IJobDbContext"/>
    protected readonly IJobDbContext dbContext = dbContext;

    /// <inheritdoc cref="IFeishuApiClient"/>
    protected readonly IFeishuApiClient feishuApiClient = feishuApiClient;

    /// <summary>
    /// 作业计划（JobScheduler）服务名
    /// </summary>
    protected abstract string JobName { get; }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "job fail, jobName: {jobName}, time: {time}, code: {code}, message: {message}")]
    protected static partial void LogOnHandleFail(ILogger logger, string jobName, string time, uint code, string? message);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message =
"""
**************************************************

JobService is executing ...

    ImpService: {serviceName}

    DbContext: {dbCtxName}
    Interfaces:
        {dbCtx}

**************************************************
""")]
    protected static partial void LogOutputDebugInfo(ILogger logger, string? serviceName, string? dbCtxName, string? dbCtx);
}

/// <summary>
/// 作业计划（JobScheduler）服务泛型基类
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
public abstract partial class JobService<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDbContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TJobService>(
        ILogger<TJobService> logger,
        TDbContext dbContext,
        IFeishuApiClient feishuApiClient) :
    JobService(
        logger,
        dbContext,
        feishuApiClient), IJob
    where TDbContext : IJobDbContext
    where TJobService : JobService<TDbContext, TJobService>
{
    /// <inheritdoc cref="ILogger"/>
    protected new ILogger<TJobService> logger = logger;

    /// <inheritdoc cref="IJobDbContext"/>
    protected new readonly TDbContext dbContext = dbContext;

    /// <summary>
    /// 是否通知
    /// </summary>
    protected virtual bool Notification => true;

    /// <summary>
    /// 是否仅在失败时通知
    /// </summary>
    protected virtual bool NotificationOnlyFail => true;

    /// <inheritdoc/>
    protected override string JobName
    {
        get
        {
            var typeName = typeof(TJobService).Name;
            const string trimChars = "JobService";
            if (typeName.Length != trimChars.Length &&
                typeName.EndsWith(trimChars, StringComparison.InvariantCultureIgnoreCase))
            {
                return typeName[..^trimChars.Length];
            }
            return typeName;
        }
    }

    /// <summary>
    /// 作业计划（JobScheduler）的业务逻辑执行入口，由子类重写实现
    /// </summary>
    protected abstract Task<ApiRsp> HandleAsync(
        IJobExecutionContext? context,
        CancellationToken cancellationToken);

    /// <summary>
    /// 作业计划（JobScheduler）的通知
    /// </summary>
    protected virtual async Task OnNotificationAsync(
        string jobName,
        JobRecordResult jobRecordResult,
        CancellationToken cancellationToken)
    {
        var title = $"JobErr: {jobName}";
        var result = await feishuApiClient.SendMessageAsync(title, jobRecordResult.Message, cancellationToken);
        jobRecordResult.Notification = result.IsSuccess();
    }

    /// <summary>
    /// 当作业计划（JobScheduler）执行完成时
    /// </summary>
    protected virtual async Task OnCompletedAsync(
        IJobExecutionContext? context,
        DateTimeOffset creationTime,
        long timestamp,
        ApiRsp result,
        CancellationToken cancellationToken)
    {
        var jobName = JobName;
        JobRecordResult jobRecordResult = new()
        {
            CreationTime = creationTime,
            Name = jobName,
            Code = result.Code,
            Message = result.Message,
            IsAutomatic = context != null,
            Elapsed = Stopwatch.GetElapsedTime(timestamp),
        };
        jobRecordResult.CompletedTime = jobRecordResult.CreationTime.Add(jobRecordResult.Elapsed);

        var isSuccess = result.IsSuccess();
        if (!isSuccess)
        {
#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
            LogOnHandleFail(logger,
                jobName,
                jobRecordResult.CreationTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
                jobRecordResult.Code,
                jobRecordResult.Message);
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
        }

        if (Notification)
        {
            if (!NotificationOnlyFail || !isSuccess)
            {
                await OnNotificationAsync(jobName, jobRecordResult, cancellationToken);
            }
        }

        await dbContext.JobRecordResults.AddAsync(jobRecordResult, cancellationToken);
        await dbContext.GetDbContext().SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc cref="IJob.Execute"/>
    public async Task<ApiRsp> ExecuteAsync(
        IJobExecutionContext? context,
        CancellationToken cancellationToken)
    {
        var timestamp = Stopwatch.GetTimestamp();
        var now = DateTimeOffset.Now;
        ApiRsp result;
        try
        {
#if DEBUG
            OutputDebugInfo();
#endif
            result = await HandleAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            result = ex;
        }
        await OnCompletedAsync(context, now, timestamp, result, cancellationToken);
        return result;
    }

    /// <inheritdoc/>
    public virtual Task Execute(IJobExecutionContext context)
    {
        return ExecuteAsync(context, context.CancellationToken);
    }

    /// <summary>
    /// 创建 Job 服务实例
    /// </summary>
    public static TJobService CreateInstance(IServiceProvider serviceProvider)
    {
        var s = JobActivatorCache.CreateInstance(serviceProvider, typeof(TJobService));
        return (TJobService)s!;
    }

#if DEBUG
    void OutputDebugInfo()
    {
        var jobDbType = typeof(TDbContext);

        var @dbInterface = string.Join(",\n \t\t", jobDbType.GetInterfaces().Select(x => x.Name));

#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
        LogOutputDebugInfo(logger, GetType().Name, jobDbType.Name, @dbInterface);
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
    }
#endif
}

file static class JobActivatorCache // https://github.com/quartznet/quartznet/blob/main/src/Quartz/Simpl/JobActivatorCache.cs
{
    static readonly ConcurrentDictionary<Type, ObjectFactory> activatorCache = new();
    static readonly Func<Type, ObjectFactory> createFactory = type => ActivatorUtilities.CreateFactory(type, Type.EmptyTypes);

    internal static object CreateInstance(
        IServiceProvider serviceProvider,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type jobType)
    {
        // job 服务无需添加进服务集合，通过反射动态创建，需要公开的单一构造函数，且构造函数参数会从服务集合中解析
        var factory = activatorCache.GetOrAdd(jobType, createFactory);
        return factory(serviceProvider, null);
    }
}