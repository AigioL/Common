using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions
{
    public interface IOrderingDbContext : IOrderingPaymentBaseDbContext, IDbContextBase
    {
        DbSet<FeeType> FeeTypes { get; }

        DbSet<Coupon> Coupons { get; }

        DbSet<UserCouponInfo> UserCouponInfos { get; }

        DbSet<AftersalesBill> AftersalesBills { get; }

        DbSet<RefundBill> RefundBills { get; }
    }
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IOrderingDbContext
    {
        public DbSet<FeeType> FeeTypes { get; set; } = null!;

        public DbSet<Coupon> Coupons { get; set; } = null!;

        public DbSet<UserCouponInfo> UserCouponInfos { get; set; } = null!;

        public DbSet<AftersalesBill> AftersalesBills { get; set; } = null!;

        public DbSet<RefundBill> RefundBills { get; set; } = null!;
    }
}
#endif
