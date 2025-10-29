using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;

public interface IOrderingPaymentBaseDbContext : IDbContextBase
{
    DbSet<Order> Orders { get; }

    DbSet<OrderPaymentComposition> OrderPaymentCompositions { get; }

    DbSet<TransferOrder> TransferOrders { get; }
}