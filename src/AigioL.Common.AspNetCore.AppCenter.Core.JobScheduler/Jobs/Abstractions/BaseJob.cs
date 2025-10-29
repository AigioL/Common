using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using Quartz;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Jobs.Abstractions;

/// <summary>
/// 作业计划（JobScheduler）的 <see cref="IJob"/> 基类
/// </summary>
public abstract class BaseJob<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDbContext,
    TJobService>(TJobService jobService) : IJob
    where TDbContext : IJobDbContext
    where TJobService : JobService<TDbContext, TJobService>
{
    /// <inheritdoc cref="JobService"/>
    protected readonly TJobService jobService = jobService;

    /// <inheritdoc/>
    public Task Execute(IJobExecutionContext context)
    {
        return jobService.ExecuteAsync(context, context.CancellationToken);
    }
}