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

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IOrderingPaymentBaseDbContext
    {
        public DbSet<Order> Orders { get; set; } = null!;

        public DbSet<OrderPaymentComposition> OrderPaymentCompositions { get; set; } = null!;

        public DbSet<TransferOrder> TransferOrders { get; set; } = null!;
    }
}
#endif