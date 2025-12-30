using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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

    public async Task<bool> CheckUserUseFirstPriceOfGoodsAsync(Guid userId, Guid goodsId, CancellationToken cancellationToken = default)
    {
        var query = db.MembershipGoodsUserFirstRecords
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId == userId && x.MembershipGoodsId == goodsId);

        var r = await query.AnyAsync(cancellationToken);
        return r;
    }
}

partial class MembershipGoodsRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<MembershipGoodsTableItem>> QueryAsync(
        Guid? id,
        string? goodsName,
        string? goodsNo,
        MembershipLicenseFlags? memberLicenseType,
        int? rechargeDays,
        decimal? currentPrice,
        bool? enable,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<MembershipGoods> query = db.MembershipGoods
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.MerchantDeductionAgreementConfigurations);

        if (id.HasValue)
            query = query.Where(x => x.Id == id);
        if (!string.IsNullOrWhiteSpace(goodsName))
            query = query.Where(x => x.GoodsName.Contains(goodsName));
        if (!string.IsNullOrWhiteSpace(goodsNo))
            query = query.Where(x => x.GoodsNo.Contains(goodsNo));
        if (memberLicenseType.HasValue)
            query = query.Where(x => x.MemberLicenseType == memberLicenseType);
        if (rechargeDays.HasValue)
            query = query.Where(x => x.RechargeDays == rechargeDays);
        if (currentPrice.HasValue)
            query = query.Where(x => x.CurrentPrice == currentPrice);
        if (enable.HasValue)
            query = query.Where(x => x.Enable == enable);

        query = query.OrderBy(x => x.CreateTime);

        var query2 = query.ProjectTo<MembershipGoodsTableItem>(mapper.ConfigurationProvider);

        var r = await query2.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<AddOrEditMembershipGoodsModel?> GetEditByIdAsync(
         Guid id,
         CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.MembershipGoods
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.MerchantDeductionAgreementConfigurations)
            .Where(x => x.Id == id)
            .ProjectTo<AddOrEditMembershipGoodsModel>(mapper.ConfigurationProvider);
        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<ApiRsp> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditMembershipGoodsModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = await FindAsync(model.Id, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        var configurations = await db.MerchantDeductionAgreementConfigurations
          .Where(x => model.Configurations.Contains(x.Id))
          .ToListAsync(cancellationToken);

        mapper.Map(model, entity);
        entity.OperatorUserId = operatorUserId;
        entity.MerchantDeductionAgreementConfigurations = configurations;

        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditMembershipGoodsModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = mapper.Map<MembershipGoods>(model);
        entity.Id = default;
        entity.CreateUserId = createUserId;

        var configurations = await db.MerchantDeductionAgreementConfigurations
          .Where(x => model.Configurations.Contains(x.Id))
          .ToListAsync(cancellationToken);
        entity.MerchantDeductionAgreementConfigurations = configurations;

        await db.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<int> EnabledMembershipGoodsAsync(Guid goodsId, bool enable, Guid operatorUserId)
    {
        var query = db.MembershipGoods
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Enable != enable && x.Id == goodsId);
        var r = await query.ExecuteUpdateAsync(e =>
            e.SetProperty(s => s.Enable, enable)
            .SetProperty(s => s.OperatorUserId, operatorUserId));
        return r;
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