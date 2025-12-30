using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Payment;

sealed partial class MerchantDeductionAgreementConfigurationRepository<TDbContext> :
    Repository<TDbContext, MerchantDeductionAgreementConfiguration, Guid>,
    IMerchantDeductionAgreementConfigurationRepository
    where TDbContext : DbContext, IPaymentDbContext
{
    public MerchantDeductionAgreementConfigurationRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class MerchantDeductionAgreementConfigurationRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<MerchantDeductionAgreementConfigurationTableItemModel>> QueryAsync(
        string? code,
        string? name,
        string? planId,
        long? period,
        string? periodType,
        decimal? firstAmount,
        decimal? singleAmount,
        PaymentType? platform,
        string? signScene,
        int? businessType,
        string? note,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.MerchantDeductionAgreementConfigurations
            .AsNoTrackingWithIdentityResolution();
        if (!string.IsNullOrEmpty(code))
            query = query.Where(x => x.Code.Contains(code));
        if (!string.IsNullOrEmpty(name))
            query = query.Where(x => x.Name.Contains(name));
        if (!string.IsNullOrEmpty(planId))
            query = query.Where(x => x.PlanId!.Contains(planId));
        if (period.HasValue)
            query = query.Where(x => x.Period == period.Value);
        if (!string.IsNullOrEmpty(periodType))
            query = query.Where(x => x.PeriodType.Contains(periodType));
        if (firstAmount.HasValue)
            query = query.Where(x => x.FirstAmount == firstAmount.Value);
        if (singleAmount.HasValue)
            query = query.Where(x => x.SingleAmount == singleAmount.Value);
        if (platform.HasValue)
            query = query.Where(x => x.Platform == platform.Value);
        if (!string.IsNullOrEmpty(signScene))
            query = query.Where(x => x.SignScene!.Contains(signScene));
        if (businessType.HasValue)
            query = query.Where(x => x.BusinessTypeId == businessType.Value);
        if (!string.IsNullOrEmpty(note))
            query = query.Where(x => x.Note!.Contains(note));
        if (createTime != null)
            query = createTime.Length switch
            {
                1 => query.Where(x => x.CreateTime >= createTime[0]),
                2 => query.Where(x => x.CreateTime >= createTime[0] && x.CreateTime <= createTime[1]),
                _ => query,
            };
        if (updateTime != null)
            query = updateTime.Length switch
            {
                1 => query.Where(x => x.UpdateTime >= updateTime[0]),
                2 => query.Where(x => x.UpdateTime >= updateTime[0] && x.UpdateTime <= updateTime[1]),
                _ => query,
            };
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }

        var r = await query.ProjectTo<MerchantDeductionAgreementConfigurationTableItemModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<AddOrEditMerchantDeductionAgreementConfigurationModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.MerchantDeductionAgreementConfigurations
            .AsNoTrackingWithIdentityResolution();

        var r = await query.Where(x => x.Id == id)
             .ProjectTo<AddOrEditMerchantDeductionAgreementConfigurationModel>(mapper.ConfigurationProvider)
             .FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditMerchantDeductionAgreementConfigurationModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = await FindAsync(model.Id, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        mapper.Map(model, entity);
        entity.OperatorUserId = operatorUserId;

        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }

    public async Task<bool> InsertAsync(
        Guid? createUserId,
        AddOrEditMerchantDeductionAgreementConfigurationModel model,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var entity = mapper.Map<MerchantDeductionAgreementConfiguration>(model);
        entity.Id = default;
        entity.CreateUserId = createUserId;

        await db.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(CancellationToken.None);
        return true;
    }
}