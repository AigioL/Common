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

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IOrderingDbContext
    {
        public DbSet<FeeType> FeeTypes { get; set; } = null!;

        public DbSet<Coupon> Coupons { get; set; } = null!;

        public DbSet<UserCouponInfo> UserCouponInfos { get; set; } = null!;

        public DbSet<AftersalesBill> AftersalesBills { get; set; } = null!;

        public DbSet<RefundBill> RefundBills { get; set; } = null!;
    }
}
#endif
