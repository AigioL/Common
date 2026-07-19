namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

/// <summary>
/// PC 提现转账状态（用于 Redis 缓存，供前端轮询）
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class PCWithdrawalTransferState
{
    /// <summary>
    /// 微信转账单据状态（ACCEPTED/PROCESSING/WAIT_USER_CONFIRM/TRANSFERING/SUCCESS/FAIL/CANCELING/CANCELLED）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// 跳转领取页面的 package 信息（仅 WAIT_USER_CONFIRM 时返回）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? PackageInfo { get; set; }
}
