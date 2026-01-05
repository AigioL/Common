using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Payment;

sealed partial class TransferOrderRepository<TDbContext> :
    Repository<TDbContext, TransferOrder, Guid>,
    ITransferOrderRepository
    where TDbContext : DbContext, IPaymentDbContext
{
    public TransferOrderRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task AddTransferOrder(TransferOrder transferOrder)
    {
        await db.TransferOrders.AddAsync(transferOrder);
        await db.SaveChangesAsync();
    }

    public async Task<TransferOrder?> GetById(Guid transferOrderId)
    {
        var query = db.TransferOrders
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == transferOrderId);

        var r = await query.FirstOrDefaultAsync();
        return r;
    }

    public async Task UpdateTransferOrderResult(TransferOrder transferOrder)
    {
        var query = db.TransferOrders
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == transferOrder.Id);

        await query.ExecuteUpdateAsync(x => x
            .SetProperty(y => y.ThirdPartyPlatformNumber, y => transferOrder.ThirdPartyPlatformNumber)
            .SetProperty(y => y.FinishTime, y => transferOrder.FinishTime)
            .SetProperty(y => y.FailureReason, y => transferOrder.FailureReason)
            .SetProperty(y => y.Note, y => transferOrder.Note)
            .SetProperty(y => y.TransferStatus, y => transferOrder.TransferStatus)
        );
    }
}
