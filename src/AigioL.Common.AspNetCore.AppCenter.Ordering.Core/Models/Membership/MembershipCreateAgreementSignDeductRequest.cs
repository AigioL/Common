using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

/// <summary>
/// 会员签约扣款请求
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial class MembershipCreateAgreementSignDeductRequest
{
    /// <summary>
    /// 用户 Id（必填）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid UserId { get; set; }

    /// <summary>
    /// 支付订单 Id（必填）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public Guid OrderId { get; set; }

    /// <summary>
    /// 扣款配置编号（必填）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public required string ConfigurationCode { get; set; }

    /// <summary>
    /// WeChatPayTradeType 枚举值（必填）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public int PaymentCode { get; set; }

    /// <summary>
    /// 支付平台类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public PaymentType Platform { get; set; }

    /// <summary>
    /// 签约协议号
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public required string AgreementNo { get; set; }

    /// <summary>
    /// 首次扣款金额
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public decimal FirstAmount { get; set; }

    /// <summary>
    /// 回调地址
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(7)]
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// 备注（最好填写）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(8)]
    public string? Note { get; set; }

    /// <inheritdoc cref="Note"/> 
    [global::MemoryPack.MemoryPackIgnore]
    public string? Remarks { get => Note; set => Note = value; } // 兼容旧数据结构
}