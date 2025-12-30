using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Payment;

sealed partial class OrderBusinessPaymentConfigurationRepository<TDbContext> :
    Repository<TDbContext, OrderBusinessPaymentConfiguration, Guid>,
    IOrderBusinessPaymentConfigurationRepository
    where TDbContext : DbContext, IPaymentDbContext
{
    public OrderBusinessPaymentConfigurationRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class OrderBusinessPaymentConfigurationRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<OrderBusinessPaymentConfigurationTableItemModel>> QueryAsync(
        int? businessType,
        PaymentMethod? paymentMethod,
        PaymentType? paymentType,
        bool? disable,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<OrderBusinessPaymentConfiguration> query = db.OrderBusinessPaymentConfigurations
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.BusinessTypeId)
            .ThenBy(x => x.PaymentMethod)
            .ThenBy(x => x.PaymentType)
            .ThenBy(x => x.Id);

        if (businessType.HasValue)
            query = query.Where(x => x.BusinessTypeId == businessType.Value);
        if (paymentMethod.HasValue)
            query = query.Where(x => x.PaymentMethod == paymentMethod.Value);
        if (paymentType.HasValue)
            query = query.Where(x => x.PaymentType == paymentType.Value);
        if (disable.HasValue)
            query = query.Where(x => x.Disable == disable.Value);
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
        if (!string.IsNullOrEmpty(createUser))
            if (ShortGuid.TryParse(createUser, out Guid createUserId))
                query = query.Where(x => x.CreateUser!.Id == createUserId);
            else
                query = query.Where(x => x.CreateUser!.NickName!.Contains(createUser));
        if (!string.IsNullOrEmpty(operatorUser))
            if (ShortGuid.TryParse(operatorUser, out Guid operatorUserId))
                query = query.Where(x => x.OperatorUser!.Id == operatorUserId);
            else
                query = query.Where(x => x.OperatorUser!.NickName!.Contains(operatorUser));

        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }

        var r = await query.ProjectTo<OrderBusinessPaymentConfigurationTableItemModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<ApiRsp> InsertAsync(
        Guid? createUserId,
        AddOrEditOrderBusinessPaymentConfigurationModel model,
        CancellationToken cancellationToken = default)
    {
        var query = db.OrderBusinessPaymentConfigurations
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.BusinessTypeId == model.BusinessTypeId &&
                x.PaymentMethod == model.PaymentMethod &&
                x.PaymentType == model.PaymentType);

        var any = await query.AnyAsync(cancellationToken);
        if (any)
        {
            return "该业务订单支付配置已存在";
        }

        OrderBusinessPaymentConfiguration entity = new()
        {
            BusinessTypeId = model.BusinessTypeId,
            PaymentMethod = model.PaymentMethod,
            PaymentType = model.PaymentType,
            Disable = model.Disable,
            CreateUserId = createUserId,
        };
        await db.OrderBusinessPaymentConfigurations.AddAsync(entity, cancellationToken);
        var rowCount = await db.SaveChangesAsync(CancellationToken.None);
        return rowCount > 0;
    }

    public async Task<int> DisableAsync(Guid? operatorUserId, Guid id, bool disable)
    {
        var r = await db.OrderBusinessPaymentConfigurations.Where(x => x.Id == id)
           .ExecuteUpdateAsync(x => x
               .SetProperty(y => y.Disable, y => disable)
               .SetProperty(y => y.UpdateTime, y => DateTimeOffset.Now)
               .SetProperty(y => y.OperatorUserId, y => operatorUserId)
           );
        return r;
    }
}
