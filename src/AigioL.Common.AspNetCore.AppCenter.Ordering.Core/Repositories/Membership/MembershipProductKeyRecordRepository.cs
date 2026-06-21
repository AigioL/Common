using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Membership;

sealed partial class MembershipProductKeyRecordRepository<TDbContext>(TDbContext dbContext, IServiceProvider serviceProvider) :
    Repository<TDbContext, MembershipProductKeyRecord, Guid>(dbContext, serviceProvider),
    IMembershipProductKeyRecordRepository
    where TDbContext : DbContext, IPaymentDbContext
{
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
        TimeSpan? rechargeTimeSpan = null,
        TimeSpan? payAsYoGo = null,
        Guid? membershipGoodsId = null,
        bool? isUsed = null,
        bool? disable = null,
        Guid? pcUserId = null,
        string? pcUserName = null,
        string? note = null,
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
#pragma warning disable CS0618 // 类型或成员已过时
            query = query.Where(x => x.RechargeDays == rechargeDays);
#pragma warning restore CS0618 // 类型或成员已过时
        if (rechargeTimeSpan.HasValue)
            query = query.Where(x => x.RechargeTimeSpan == rechargeTimeSpan);
        if (payAsYoGo.HasValue)
            query = query.Where(x => x.PayAsYoGo == payAsYoGo);
        if (pcUserId.HasValue)
            query = query.Where(x => x.PCUserId == pcUserId);
        else if (!string.IsNullOrEmpty(pcUserName))
        {
            if (pcUserName.Length == 11 && pcUserName.All(char.IsAsciiDigit))
            {
                query = query.Where(x => x.PCUser != null && x.PCUser.PhoneNumber == pcUserName);
            }
            else
            {
                query = query.Where(x =>
                    x.PCUser != null &&
                    ((x.PCUser.NickName != null && x.PCUser.NickName.Contains(pcUserName)) ||
                        (x.PCUser.UserName != null && x.PCUser.UserName.Contains(pcUserName))));
            }
        }
        if (!string.IsNullOrEmpty(note))
            query = query.Where(x => x.Note == note);

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

    public async Task<ApiRsp<string[]?>> BatchCreateProductKeyRecordAsync(
        Guid createUserId,
        Guid membershipGoodsId,
        uint count,
        Guid? pcUserId,
        string? note,
        CancellationToken cancellationToken = default)
    {
        if (membershipGoodsId == default)
        {
            return "商品 Id 不能为空";
        }
        var membershipGoods = await db.MembershipGoods.Select(x => new
        {
            x.Id,
            x.RechargeTimeSpan,
            x.PayAsYoGo,
            x.MemberLicenseType,
        }).FirstOrDefaultAsync(x => x.Id == membershipGoodsId, cancellationToken);
        if (membershipGoods == null)
        {
            return "找不到商品 Id";
        }
        if (!membershipGoods.MemberLicenseType.HasFlag(MembershipLicenseFlags.CDKey))
        {
            return "商品的会员订阅类型不支持 CDKey";
        }

        var records = new MembershipProductKeyRecord[count];
        for (long i = 0; i < records.LongLength; i++)
        {
            records[i] = new MembershipProductKeyRecord()
            {
                Id = Guid.NewGuid(), // 这里要保证随机性，不要使用 Guid v7
                RechargeTimeSpan = membershipGoods.RechargeTimeSpan,
                PayAsYoGo = membershipGoods.PayAsYoGo,
                MembershipGoodsId = membershipGoods.Id,
                IsUsed = false,
                UsageTime = null,
                Disable = false,
                CreateUserId = createUserId,
                PCUserId = pcUserId,
                Note = note,
            };
        }

        await db.MembershipProductKeyRecords.AddRangeAsync(records, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var query = from m in records
                    let s = Base58Guid.Encode(m.Id)
                    select s;
        return query.ToArray();
    }

    public async Task<int> BatchDisableProductKeyRecordAsync(
        Guid? operatorUserId,
        bool disable,
        params IEnumerable<Guid> keys)
    {
        var query = db.MembershipProductKeyRecords
            .IgnoreQueryFilters()
            .Where(x => keys.Contains(x.Id));

        var r = await query.ExecuteUpdateAsync(e =>
            e.SetProperty(p => p.Disable, disable)
            .SetProperty(p => p.OperatorUserId, operatorUserId)
            .SetProperty(p => p.UpdateTime, DateTimeOffset.UtcNow));
        return r;
    }

    public async Task<int> BatchDisableProductKeyRecordAsync(
        Guid? operatorUserId,
        bool disable,
        params IEnumerable<string> keys)
    {
        var keysQuery = from m in keys
                        let g = Base58Guid.Decode(m)
                        where g.HasValue
                        select g.Value;
        var keysG = keysQuery.ToArray();

        var r = await BatchDisableProductKeyRecordAsync(operatorUserId, disable, keysG);
        return r;
    }
}