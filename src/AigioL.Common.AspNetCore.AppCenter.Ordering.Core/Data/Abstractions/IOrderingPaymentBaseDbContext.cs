using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions
{
    public interface IOrderingPaymentBaseDbContext : IDbContextBase
    {
        DbSet<Order> Orders { get; }

        DbSet<OrderPaymentComposition> OrderPaymentCompositions { get; }

        DbSet<TransferOrder> TransferOrders { get; }
    }
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IOrderingPaymentBaseDbContext
    {
        public DbSet<Order> Orders { get; set; } = null!;

        public DbSet<OrderPaymentComposition> OrderPaymentCompositions { get; set; } = null!;

        public DbSet<TransferOrder> TransferOrders { get; set; } = null!;
    }
}
#endif