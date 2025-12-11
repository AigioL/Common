using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;

public interface IMembershipGoodsRepository : IRepository<MembershipGoods, Guid>, IEFRepository
{
    /// <summary>
    /// 获取上架的会员商品
    /// </summary>
    Task<MembershipGoodsModel[]> GetMembershipGoodsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户检查商品价格，检查用户是否使用过首次优惠
    /// </summary>
    Task<MembershipGoodsModel[]> CheckPriceByUserAsync(Guid userId, MembershipGoodsModel[] goodsArray, CancellationToken cancellationToken = default);
}
