namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

public sealed record class PubPayState
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 支付 Url
    /// </summary>
    public Uri? Url { get; set; }

    public static implicit operator PubPayState(string message) => new() { Message = message };

    public static implicit operator PubPayState(Uri url) => new() { Url = url, IsSuccess = true, };
}
