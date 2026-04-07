using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;

public partial interface IMembershipGoodsRepository : IRepository<MembershipGoods, Guid>, IEFRepository
{
    /// <summary>
    /// 获取上架的会员商品
    /// </summary>
    Task<MembershipGoodsModel[]> GetMembershipGoodsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户检查商品价格，检查用户是否使用过首次优惠
    /// </summary>
    Task<MembershipGoodsModel[]> CheckPriceByUserAsync(Guid userId, MembershipGoodsModel[] goodsArray, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户是否使用过商品的首次优惠
    /// </summary>
    Task<bool> CheckUserUseFirstPriceOfGoodsAsync(Guid userId, Guid goodsId, CancellationToken cancellationToken = default);
}

partial interface IMembershipGoodsRepository
{
    Task<PagedModel<MembershipGoodsTableItem>> QueryAsync(
        Guid? id,
        string? goodsName,
        string? goodsNo,
        MembershipLicenseFlags? memberLicenseType,
        int? rechargeDays,
        TimeSpan? rechargeTimeSpan,
        TimeSpan? payAsYoGo,
        decimal? currentPrice,
        bool? enable,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<AddOrEditMembershipGoodsModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditMembershipGoodsModel model,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditMembershipGoodsModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 上架或下架会员商品
    /// </summary>
    Task<int> EnabledMembershipGoodsAsync(Guid goodsId, bool enable, Guid operatorUserId);
}