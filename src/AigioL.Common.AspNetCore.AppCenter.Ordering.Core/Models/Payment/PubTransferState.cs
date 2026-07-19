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

    /// <summary>
    /// 微信转账原始状态（ACCEPTED/PROCESSING/WAIT_USER_CONFIRM/TRANSFERING/SUCCESS/FAIL/CANCELING/CANCELLED）
    /// </summary>
    public string? WeChatState { get; set; }

    /// <summary>
    /// 跳转领取页面的 package 信息（仅 WAIT_USER_CONFIRM 时返回）
    /// </summary>
    public string? PackageInfo { get; set; }
}