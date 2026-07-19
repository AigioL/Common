namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 微信提现转账完成通知信息（通过消息队列传递）
/// </summary>
/// <param name="OutBillNo">商户转账单号（对应 WithdrawalNumber）</param>
/// <param name="TransferBillNo">微信转账单号</param>
/// <param name="State">单据状态：SUCCESS/FAIL/CANCELLED</param>
/// <param name="TransferAmount">转账金额（单位：分）</param>
/// <param name="FailReason">失败原因（终态为 FAIL 时返回）</param>
public sealed record WithdrawalTransferCompletedInfo(
    string OutBillNo,
    string TransferBillNo,
    string State,
    int TransferAmount,
    string? FailReason);
