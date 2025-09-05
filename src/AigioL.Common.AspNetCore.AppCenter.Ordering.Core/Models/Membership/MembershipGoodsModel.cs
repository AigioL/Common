using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

/// <summary>
/// 会员商品模型
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public partial record class MembershipGoodsModel
{
    [global::MemoryPack.MemoryPackOrder(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string GoodsName { get; set; } = string.Empty;

    /// <summary>
    /// 商品编号
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public string GoodsNo { get; set; } = string.Empty;

    /// <summary>
    /// 订阅类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public MembershipLicenseFlags MemberLicenseType { get; set; }

    /// <summary>
    /// 充值天数
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(4)]
    public int RechargeDays { get; set; }

    /// <summary>
    /// 首次购买价格（原价）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public decimal? FirstPrice { get; set; }

    /// <summary>
    /// 首次购买价格（现价）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public decimal? FirstCurrentPrice { get; set; }

    /// <summary>
    /// 商品价格（原价）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(7)]
    public decimal Price { get; set; }

    /// <summary>
    /// 商品价格（现价）
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(8)]
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(9)]
    public string? Note { get; set; }

    /// <inheritdoc cref="Note"/> 
    [global::MemoryPack.MemoryPackIgnore]
    public string? Remarks { get => Note; set => Note = value; } // 兼容旧数据结构

    /// <summary>
    /// 扣款协议配置列表
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(10)]
    public List<MerchantDeductionConfigurationInfo> Configurations { get; set; } = [];
}