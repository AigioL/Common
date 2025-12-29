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
}
