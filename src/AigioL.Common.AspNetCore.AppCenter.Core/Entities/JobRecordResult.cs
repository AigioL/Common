using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Entities;

/// <summary>
/// 作业计划（JobScheduler）记录执行结果实体类
/// </summary>
public sealed partial class JobRecordResult :
    Entity<Guid>,
    ICreationTime,
    INEWSEQUENTIALID
{
    /// <inheritdoc/>
    [Comment("创建时间")]
    public DateTimeOffset CreationTime { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Comment("名称")]
    [StringLength(MaxLengths.LongName)]
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// 执行结果状态码
    /// <list type="bullet">
    /// <item><see cref="HttpStatusCode.OK"/> 执行成功</item>
    /// <item><see cref="HttpStatusCode.BadRequest"/> 执行失败（已知的错误，见 <see cref="ApiRsp.Message"/>）</item>
    /// <item><see cref="HttpStatusCode.InternalServerError"/> 执行出现 catch 引发 <see cref="Exception"/></item>
    /// </list>
    /// </summary>
    [Comment("执行结果状态码")]
    public uint Code { get; set; }

    /// <summary>
    /// 执行结果消息
    /// </summary>
    [Comment("执行结果消息")]
    public string? Message { get; set; }

    /// <summary>
    /// 是否通知成功
    /// <list type="bullet">
    /// <item><see langword="null"/> 不需要通知</item>
    /// <item><see langword="true"/> 通知成功</item>
    /// <item><see langword="false"/> 通知失败</item>
    /// </list>
    /// </summary>
    [Comment("是否通知成功")]
    public bool? Notification { get; set; }

    /// <summary>
    /// 执行完成时间
    /// </summary>
    [Comment("执行完成时间")]
    public DateTimeOffset CompletedTime { get; set; }

    /// <summary>
    /// 执行耗时
    /// </summary>
    [Comment("执行耗时")]
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// 是否为自动执行的作业计划（JobScheduler）任务，非调用 API 调用触发执行的
    /// </summary>
    [Comment("是否自动执行")]
    public bool IsAutomatic { get; set; }
}
