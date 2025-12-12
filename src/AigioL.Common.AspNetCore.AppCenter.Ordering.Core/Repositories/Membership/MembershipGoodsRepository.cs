using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Membership;

sealed partial class MembershipGoodsRepository<TDbContext> :
    Repository<TDbContext, MembershipGoods, Guid>,
    IMembershipGoodsRepository
    where TDbContext : DbContext, IPaymentDbContext
{
    public MembershipGoodsRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<MembershipGoodsModel[]> GetMembershipGoodsAsync(CancellationToken cancellationToken = default)
    {
        var query = db.MembershipGoods
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.MerchantDeductionAgreementConfigurations)
            .Where(x => x.Enable)
            .OrderBy(x => x.MemberLicenseType)
            .ThenBy(x => x.Price)
            .Select(ProjectToMapper.MembershipGoodsModelExpr);

        var r = await query.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task<MembershipGoodsModel[]> CheckPriceByUserAsync(Guid userId, MembershipGoodsModel[] goodsArray, CancellationToken cancellationToken = default)
    {
        var goodsIds = goodsArray.Select(x => x.Id).ToArray();
        var hashSets = await db.MembershipGoodsUserFirstRecords
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId == userId && goodsIds.Contains(x.MembershipGoodsId))
            .Select(s => s.MembershipGoodsId)
            .ToHashSetAsync(cancellationToken);

        foreach (var item in goodsArray)
        {
            if (hashSets.Contains(item.Id))
            {
                item.FirstPrice = null;
                item.FirstCurrentPrice = null;
            }
        }

        return goodsArray;
    }
}

file static class ProjectToMapper
{
    internal static readonly Expression<Func<MembershipGoods, MembershipGoodsModel>> MembershipGoodsModelExpr = it => new()
    {
        Id = it.Id,
        GoodsName = it.GoodsName,
        GoodsNo = it.GoodsNo,
        MemberLicenseType = it.MemberLicenseType,
        RechargeDays = it.RechargeDays,
        FirstPrice = it.FirstPrice,
        FirstCurrentPrice = it.FirstCurrentPrice,
        Price = it.Price,
        CurrentPrice = it.CurrentPrice,
        Note = it.Note,
        Configurations = it.MerchantDeductionAgreementConfigurations.Select(c => new MerchantDeductionConfigurationInfo
        {
            Id = c.Id,
            Code = c.Code,
            Platform = c.Platform,
        }).ToList(),
    };
}