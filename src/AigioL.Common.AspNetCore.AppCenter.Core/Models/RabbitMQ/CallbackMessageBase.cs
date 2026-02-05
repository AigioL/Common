namespace AigioL.Common.AspNetCore.AppCenter.Models.RabbitMQ;

public class CallbackMessageBase : RetryMessageBase
{
    /// <summary>
    /// 回调路由 Key
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// 回调任务 Id
    /// </summary>
    public Guid? TaskId { get; set; }
}
