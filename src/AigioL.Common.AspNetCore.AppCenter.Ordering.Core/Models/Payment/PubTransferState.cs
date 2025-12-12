namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

public sealed partial record class PubTransferState
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
    /// 第三方平台转账订单号
    /// </summary>
    public string? ThirdPartyPlatformNumber { get; set; }

    /// <summary>
    /// 支付宝支付资金流水号
    /// </summary>
    public string? AlipayPayFundOrderId { get; set; }

    /// <summary>
    /// 转账状态
    /// </summary>
    public TransferStatus TransferStatus { get; set; }

    /// <summary>
    /// 转账完成时间
    /// </summary>
    public DateTimeOffset? FinishTime { get; set; }
}