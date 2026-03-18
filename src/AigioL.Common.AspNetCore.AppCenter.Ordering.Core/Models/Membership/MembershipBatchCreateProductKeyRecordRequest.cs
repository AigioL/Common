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
}
