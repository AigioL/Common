using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;

public interface IOrderingDbContext : IOrderingPaymentBaseDbContext, IDbContextBase
{
    DbSet<FeeType> FeeTypes { get; }

    DbSet<Coupon> Coupons { get; }

    DbSet<UserCouponInfo> UserCouponInfos { get; }

    DbSet<AftersalesBill> AftersalesBills { get; }

    DbSet<RefundBill> RefundBills { get; }
}
