namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

/// <summary>
/// 批量创建会员产品密钥请求
/// </summary>
public sealed partial class MembershipBatchCreateProductKeyRecordRequest
{
    /// <summary>
    /// 会员商品 Id
    /// </summary>
    public Guid MembershipGoodsId { get; set; }

    /// <summary>
    /// 批量创建数量
    /// </summary>
    public uint Count { get; set; }

    /// <summary>
    /// 分成 KOL 用户 Id
    /// </summary>
    public Guid? RevenueShareRecipientKolUserId { get; set; }

    /// <summary>
    /// 绑定的合作伙伴用户到期时间
    /// </summary>
    public DateTimeOffset BindPCUserExpireDate { get; set; }

    /// <summary>
    /// 分成比例
    /// </summary>
    public decimal RevenueSharePercentage { get; set; }
}
