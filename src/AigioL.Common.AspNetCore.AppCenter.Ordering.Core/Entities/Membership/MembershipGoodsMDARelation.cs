using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;

/// <summary>
/// 会员商品扣款协议配置多对多关系表实体类
/// </summary>
[Table(nameof(MembershipGoodsMDARelation) + "s")]
public partial class MembershipGoodsMDARelation
{
    public Guid MembershipGoodsId { get; set; }

    public virtual MembershipGoods MembershipGoods { get; set; } = null!;

    public Guid MerchantDeductionAgreementConfigurationId { get; set; }

    public virtual MerchantDeductionAgreementConfiguration MerchantDeductionAgreementConfiguration { get; set; } = null!;
}