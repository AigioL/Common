namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 扣款协议配置
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public partial record class MerchantDeductionConfigurationInfo
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    [global::MemoryPack.MemoryPackOrder(1)]
    public string Code { get; set; } = string.Empty;

    [global::MemoryPack.MemoryPackOrder(2)]
    public PaymentType Platform { get; set; }
}
