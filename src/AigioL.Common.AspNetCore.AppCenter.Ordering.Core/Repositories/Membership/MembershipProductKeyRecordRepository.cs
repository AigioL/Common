using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
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
