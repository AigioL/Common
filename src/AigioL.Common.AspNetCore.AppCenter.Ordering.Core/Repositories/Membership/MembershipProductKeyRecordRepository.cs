using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Membership;

sealed partial class MembershipProductKeyRecordRepository<TDbContext> :
    Repository<TDbContext, MembershipProductKeyRecord, Guid>,
    IMembershipProductKeyRecordRepository
    where TDbContext : DbContext, IPaymentDbContext
{
    public MembershipProductKeyRecordRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<MembershipProductKeyRecord?> GetProductKeyRecord(Guid recordId, bool? disable, bool? isUsed, CancellationToken cancellationToken = default)
    {
        IQueryable<MembershipProductKeyRecord> query = db.MembershipProductKeyRecords
            .AsNoTrackingWithIdentityResolution()
           .Where(x => x.Id == recordId)
           .Include(x => x.MembershipGoods);

        if (disable.HasValue)
            query = query.Where(x => x.Disable == disable);
        if (isUsed.HasValue)
            query = query.Where(x => x.IsUsed == isUsed);

        var record = await query.FirstOrDefaultAsync(cancellationToken);
        return record;
    }
}

partial class MembershipProductKeyRecordRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<MembershipProductKeyRecordTableItem>> QueryAsync(
        Guid? key = null,
        int? rechargeDays = null,
        Guid? membershipGoodsId = null,
        bool? isUsed = null,
        bool? disable = null,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<MembershipProductKeyRecord> query = db.MembershipProductKeyRecords
            .AsNoTrackingWithIdentityResolution()
            .Include(s => s.MembershipGoods);

        if (membershipGoodsId.HasValue)
            query = query.Where(x => x.MembershipGoodsId == membershipGoodsId);

        if (isUsed.HasValue)
            query = query.Where(x => x.IsUsed == isUsed);

        if (disable.HasValue)
            query = query.Where(x => x.Disable == disable);

        if (key.HasValue)
            query = query.Where(x => x.Id == key.Value);

        if (rechargeDays.HasValue)
            query = query.Where(x => x.RechargeDays == rechargeDays);
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderBy(x => x.Disable)
             .ThenBy(x => x.IsUsed)
             .ThenByDescending(x => x.CreateTime);
        }

        var r = await query
             .ProjectTo<MembershipProductKeyRecordTableItem>(mapper.ConfigurationProvider)
             .PagingAsync(current, pageSize, RequestAborted);

        return r;
    }
}