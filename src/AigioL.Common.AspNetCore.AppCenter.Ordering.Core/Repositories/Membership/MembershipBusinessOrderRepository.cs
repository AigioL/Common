using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Helpers.SnowFlake;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using static AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Membership.LogMembershipBusinessOrderRepository;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Membership;

public partial class MembershipBusinessOrderRepository<TDbContext> :
    Repository<TDbContext, MembershipBusinessOrder, Guid>,
    IMembershipBusinessOrderRepository
    where TDbContext : DbContext, IPaymentDbContext, IIdentityDbContext
{
    readonly ILogger logger;
    readonly IOrderBusinessTypeService orderBusinessTypeService;

    public MembershipBusinessOrderRepository(
        IOrderBusinessTypeService orderBusinessTypeService,
        ILogger<MembershipBusinessOrderRepository<TDbContext>> logger,
        TDbContext dbContext, IServiceProvider serviceProvider) :
        base(dbContext, serviceProvider)
    {
        this.logger = logger;
        this.orderBusinessTypeService = orderBusinessTypeService;
    }

    /// <summary>
    /// 订单业务类型（会员业务）
    /// </summary>
    protected virtual int OrderBusinessType => orderBusinessTypeService.Membership;

    #region CreateMembershipBusinessOrder / 创建业务订单

    /// <inheritdoc/>
    public async Task<(bool Success, Order? Order, UserMembershipChangeRecord? record)> CreateBusinessOrder(
        MembershipBusinessOrder business_order,
        bool isAgreementDeduction = false,
        PaymentType? paymentType = null,
        string? orderId = null,
        (Guid bindPCUserId, TimeSpan? bindPCUserExpirePeriod)? bindPCUser = null)
    {
        // 自动续费订单
        if (isAgreementDeduction)
        {
            if (paymentType is null)
                throw new ArgumentNullException(nameof(paymentType));

            var order = await CreateMembershipBusinessOrderByAgreementAsync(
                business_order,
                paymentType!.Value,
                orderId);
            return (order != null, order, null);
        }
        // CDKey 兑换
        else if (business_order.BusinessSource == MembershipBusinessSource.CDK激活)
        {
            var (isOK, record) = await CreateMembershipBusinessOrderByCDKeyAsync(business_order, bindPCUser: bindPCUser);
            return (isOK, null, record);
        }
        // 普通订单
        else
        {
            var order = await CreateMembershipBusinessOrderAsync(business_order, orderId);
            return (order != null, order, null);
        }
    }

    #endregion

    public async Task<MembershipBusinessOrder?> NoTrackingFirstOrDefaultAsync(Expression<Func<MembershipBusinessOrder, bool>> expression)
    {
        var query = db.MembershipBusinessOrders.AsNoTrackingWithIdentityResolution().Where(expression);
        var r = await query.FirstOrDefaultAsync();
        return r;
    }

    public async Task<GoodsRechargeStatus?> GetOrderStatusByProductKeyRecord(Guid productKeyRecordId)
    {
        var query = db.MembershipBusinessOrders
             .AsNoTrackingWithIdentityResolution()
             .Where(x => x.ProductKeyRecordId == productKeyRecordId && x.BusinessSource == MembershipBusinessSource.CDK激活)
             .Select(x => x.GoodsRechargeStatus);
        var r = await query.FirstOrDefaultAsync();
        return r;
    }

    /// <summary>
    /// 用户退款成功，充值相应撤回
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public async Task<(bool isSuccess, Guid? userId)> OrderRefunded(string orderId)
    {
        var business_order = await db.MembershipBusinessOrders
             .AsNoTrackingWithIdentityResolution()
             .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (business_order is null)
            return (false, null);

        var isSuccess = await db.Database.CreateExecutionStrategy().ExecuteAsync(OrderRefundedCoreAsync);
        return (isSuccess, business_order.UserId);

        async Task<bool> OrderRefundedCoreAsync()
        {
            using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                var r = await db.MembershipBusinessOrders
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x =>
                        x.OrderId == orderId &&
                        x.PaymentStatus == OrderStatus.Paid)
                    .ExecuteUpdateAsync(e => e
                        .SetProperty(s => s.PaymentStatus, OrderStatus.Refunded)
                        .SetProperty(s => s.GoodsRechargeStatus, s => GoodsRechargeStatus.RechargeReturn));

                var membershipChangeSuccess = await UserMembershipRechargeReturnAsync(business_order);

                await DeleteUserFirstPriceOfGoods();
                if (r > 0 && membershipChangeSuccess)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                throw new InvalidOperationException();
            }
            catch (Exception)
            {
                return false;
            }

            // 去除用户该商品首次优惠记录，如果有
            async Task DeleteUserFirstPriceOfGoods()
            {
                await db.MembershipGoodsUserFirstRecords
                    .AsNoTracking()
                    .Where(x =>
                        x.UserId == business_order.UserId &&
                        x.MembershipGoodsId == business_order.MembershipGoodsId &&
                        x.MembershipBusinessOrderId == business_order.Id)
                    .ExecuteDeleteAsync();
            }
        }
    }

    public async Task<(bool isSuccess, Guid? userId)> OrderPaymentSuccess(string orderId)
    {
        var order = await db.Orders.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(x => x.Id == orderId);

        if (order != null && order.Status == OrderStatus.Paid)
        {
            var business_order = await db.MembershipBusinessOrders
                .AsNoTrackingWithIdentityResolution()
                .Include(i => i.MembershipGoods)
                .Where(x => x.Id == order.BusinessOrderId && x.UserId == order.UserId && x.PaymentStatus == OrderStatus.WaitPay)
                .FirstAsync();

            var isSuccess = await db.Database.CreateExecutionStrategy().ExecuteAsync(OrderPaymentSuccessCore);
            return (isSuccess, business_order.UserId);

            async Task<bool> OrderPaymentSuccessCore()
            {
                using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
                try
                {
                    var now = DateTimeOffset.Now;
                    var r = await db.MembershipBusinessOrders
                        .AsNoTrackingWithIdentityResolution()
                        .Where(x => x.Id == order.BusinessOrderId && x.UserId == order.UserId && x.PaymentStatus == OrderStatus.WaitPay)
                        .ExecuteUpdateAsync(e =>
                                e.SetProperty(s => s.PaymentStatus, order.Status)
                                .SetProperty(s => s.PaymentTime, order.PaymentTime)
                                .SetProperty(s => s.AmountReceived, order.AmountReceived)
                                .SetProperty(s => s.GoodsRechargeStatus, GoodsRechargeStatus.Recharged)
                                .SetProperty(s => s.RechargeCompletionTime, now)) > 0;

                    var (rowCount, _) = await CreateOrUpdateUserMembershipAsync(business_order, now);
                    var userMembershipChangeSuccess = rowCount > 0;

                    if (r && userMembershipChangeSuccess && await CheckUserUseFistPriceAsync())
                    {
                        await transaction.CommitAsync();
                        return true;
                    }

                    throw new InvalidOperationException();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // 检查是否使用该商品首次优惠
                async Task<bool> CheckUserUseFistPriceAsync()
                {
                    var exists = await db.MembershipGoodsUserFirstRecords.AsNoTracking()
                        .AnyAsync(x =>
                            x.UserId == business_order.UserId &&
                            x.MembershipGoodsId == business_order.MembershipGoodsId);
                    if (!exists)
                    {
                        var amountReceivable = business_order.AmountReceivable;
                        if (amountReceivable == business_order.MembershipGoods!.FirstCurrentPrice ||
                           amountReceivable == business_order.MembershipGoods.FirstPrice)
                        {
                            db.MembershipGoodsUserFirstRecords.Add(new MembershipGoodsUserFirstRecord
                            {
                                UserId = business_order.UserId,
                                MembershipGoodsId = business_order.MembershipGoodsId,
                                MembershipBusinessOrderId = business_order.Id
                            });
                            return await db.SaveChangesAsync() > 0;
                        }
                    }

                    return true;
                }
            }
        }

        return (false, null);
    }

    public async Task<MembershipBusinessOrder?> GetBusinessOrderByAgreementAsync(Guid agreementId)
    {
        var query = db.MembershipBusinessOrders
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(x => x.CreateTime)
            .Where(x => x.MerchantDeductionAgreementId == agreementId);
        var r = await query.FirstOrDefaultAsync();
        return r;
    }

    public async Task<(bool isSuccess, decimal? firstAmount)> AddAgreementAndBindOrderAsync(
        MerchantDeductionAgreement agreement,
        string orderId)
    {
        var firstAmount = await db.Orders
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == orderId).Select(x => x.AmountReceivable)
            .FirstOrDefaultAsync();

        var isSuccess = await db.Database.CreateExecutionStrategy().ExecuteAsync(AddAgreementAndBindOrderCore);

        return (isSuccess, firstAmount);

        async Task<bool> AddAgreementAndBindOrderCore()
        {
            using var transaction = await db.Database.BeginTransactionAsync(RequestAborted);
            try
            {
                agreement.FirstAmount = firstAmount;
                db.MerchantDeductionAgreements.Add(agreement);
                var r = await db.SaveChangesAsync() > 0;

                var r1 = await db.MembershipBusinessOrders
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x => x.OrderId == orderId && x.MerchantDeductionAgreementId == null)
                    .ExecuteUpdateAsync(e => e.SetProperty(s => s.MerchantDeductionAgreementId, agreement.Id)) > 0;

                var r2 = await db.Orders
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x => x.Id == orderId && x.MerchantDeductionAgreementId == null)
                    .ExecuteUpdateAsync(e => e.SetProperty(s => s.MerchantDeductionAgreementId, agreement.Id)) > 0;
                if (r && r1 && r2)
                {
                    await transaction.CommitAsync();
                    return true;
                }
                throw new InvalidOperationException();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }

    static void SetBindPCUser(
        UserMembership userMembership,
        (Guid bindPCUserId, TimeSpan? bindPCUserExpirePeriod)? bindPCUser = null)
    {
        if (bindPCUser.HasValue)
        {
            userMembership.BindPCUserId = bindPCUser.Value.bindPCUserId;
            userMembership.BindPCUserExpireDate = bindPCUser.Value.bindPCUserExpirePeriod.HasValue ?
                DateTimeOffset.UtcNow.Add(bindPCUser.Value.bindPCUserExpirePeriod.Value) :
                DateTimeOffset.MaxValue;
        }
    }

    public async Task<(int rowCount, UserMembershipChangeRecord? record)> CreateOrUpdateUserMembershipAsync(
        Guid businessOrderId,
        TimeSpan rechargeTimeSpan,
        TimeSpan payAsYoGo,
        Guid userId,
        MembershipLicenseFlags membershipLicenseFlags,
        MembershipBusinessSource membershipBusinessSource,
        DateTimeOffset? now_ = null,
       (Guid bindPCUserId, TimeSpan? bindPCUserExpirePeriod)? bindPCUser = null)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("必须在事务中调用此方法");
        }

        var now = now_ ?? DateTimeOffset.Now;

        var userMembership = await db.UserMemberships.FirstOrDefaultAsync(x => x.Id == userId);

        DateTimeOffset currentRealExpireDate;

        var userMembershipChangeSuccess = false;
        if (userMembership == null)
        {
            currentRealExpireDate = now.Add(rechargeTimeSpan);

            (bool flags, userMembership) = await CreateNewUserMembership(
                userId, now, currentRealExpireDate,
                membershipLicenseFlags, payAsYoGo,
                bindPCUser);
            if (flags)
            {
                flags = await AddUserTypeAsync(userId, UserType.Membership);
                userMembershipChangeSuccess = flags;
            }
        }
        else
        {
            bool changeUserTypeSuccess = false;
            // 之前的会员已经到期
            if (userMembership.ExpireDate <= now)
            {
                currentRealExpireDate = now.Add(rechargeTimeSpan);
                userMembership.StartDate = now;
                userMembership.ExpireDate = currentRealExpireDate;
            }
            else // 在会员期内购买会员
            {
                currentRealExpireDate = userMembership.ExpireDate.Add(rechargeTimeSpan);
                userMembership.ExpireDate = currentRealExpireDate;
            }

            changeUserTypeSuccess = await AddUserTypeAsync(userId, UserType.Membership);

            var membershipLicenseType = ConvertMembershipLicenseType(membershipLicenseFlags, membershipBusinessSource);
            userMembership.MemberLicenseFlags |= membershipLicenseType;
            userMembership.PayAsYoGo += payAsYoGo;
            SetBindPCUser(userMembership, bindPCUser);
            db.UserMemberships.Update(userMembership);

            var rowCount = await db.SaveChangesAsync();
            userMembershipChangeSuccess = rowCount > 0 && changeUserTypeSuccess;
        }

        if (userMembershipChangeSuccess)
        {
            var record = new UserMembershipChangeRecord()
            {
                UserId = userId,
                MembershipChangeDirection = MembershipChangeDirection.In,
                Value = rechargeTimeSpan,
                PayAsYoGo = payAsYoGo,
                Note = membershipLicenseFlags.GetDescription(),
                MemberLicenseType = membershipLicenseFlags,
                CreateTime = now,
                CurrentRealExpireDate = currentRealExpireDate,
                BindPCUserId = userMembership.BindPCUserId,
                BindPCUserExpireDate = userMembership.BindPCUserExpireDate,
            };

            db.UserMembershipChangeRecords.Add(record);
            var rowCount = await db.SaveChangesAsync();
            return (rowCount, record);
        }

        return (0, null);
    }

    #region Private Methods

    Task<(int rowCount, UserMembershipChangeRecord? record)> CreateOrUpdateUserMembershipAsync(
        MembershipBusinessOrder business_order,
        DateTimeOffset? now,
        (Guid bindPCUserId, TimeSpan? bindPCUserExpirePeriod)? bindPCUser = null)
    {
        var rechargeTimeSpan = business_order.RechargeTimeSpan;
#pragma warning disable CS0618 // 类型或成员已过时
        if (rechargeTimeSpan == default && business_order.RechargeDays != default)
        {
            rechargeTimeSpan = TimeSpan.FromDays(business_order.RechargeDays);
        }
#pragma warning restore CS0618 // 类型或成员已过时
        return CreateOrUpdateUserMembershipAsync(
            business_order.Id, rechargeTimeSpan, business_order.PayAsYoGo, business_order.UserId,
            business_order.MemberLicenseType, business_order.BusinessSource, now,
            bindPCUser: bindPCUser);
    }

    /// <summary>
    /// 首次购买会员
    /// </summary>
    async Task<(bool isOk, UserMembership userMembership)> CreateNewUserMembership(
        Guid userId,
        DateTimeOffset now,
        DateTimeOffset currentRealExpireDate,
        MembershipLicenseFlags membershipLicenseFlags,
        TimeSpan payAsYoGo,
       (Guid bindPCUserId, TimeSpan? bindPCUserExpirePeriod)? bindPCUser = null)
    {
        var userMembership = new UserMembership()
        {
            Id = userId,
            StartDate = now,
            ExpireDate = currentRealExpireDate,
            MemberLicenseFlags = membershipLicenseFlags,
            FirstMembershipDate = now,
            PayAsYoGo = payAsYoGo,
        };
        SetBindPCUser(userMembership, bindPCUser);
        await db.UserMemberships.AddAsync(userMembership);
        var rowCount = await db.SaveChangesAsync();
        return (rowCount > 0, userMembership);
    }

    /// <summary>
    /// 添加会员用户类型，已经是会员则忽略修改
    /// </summary>
    async Task<bool> AddUserTypeAsync(Guid userId, UserType userTypeVal = UserType.Membership)
    {
        var exists = await db.Users.AnyAsync(x => x.Id == userId && x.UserType.HasFlag(userTypeVal));
        if (!exists)
        {
            var userType = await GetUserQueryWithHasFlag(userId, userTypeVal)
                .Select(x => (UserType?)x.UserType)
                .FirstOrDefaultAsync();
            if (userType.HasValue)
            {
                userType = userType.Value | userTypeVal;
                var r = await GetUserQueryWithHasFlag(userId, userTypeVal)
                    .ExecuteUpdateAsync(s => s.SetProperty(e => e.UserType, e => userType.Value));
            }
        }

        return true;
    }

    /// <summary>
    /// 根据业务订单撤销用户会员充值
    /// </summary>
    Task<bool> UserMembershipRechargeReturnAsync(MembershipBusinessOrder business_order)
    {
        var rechargeTimeSpan = business_order.RechargeTimeSpan;
#pragma warning disable CS0618 // 类型或成员已过时
        if (rechargeTimeSpan == default && business_order.RechargeDays != default)
        {
            rechargeTimeSpan = TimeSpan.FromDays(business_order.RechargeDays);
        }
#pragma warning restore CS0618 // 类型或成员已过时
        rechargeTimeSpan = rechargeTimeSpan.Negate();
        var payAsYoGo = business_order.PayAsYoGo.Negate();
        return UserMembershipRechargeReturnAsync(
            business_order.Id, rechargeTimeSpan, payAsYoGo, business_order.UserId,
            business_order.MemberLicenseType, business_order.BusinessSource);
    }

    async Task<bool> UserMembershipRechargeReturnAsync(
        Guid businessOrderId,
        TimeSpan rechargeTimeSpan,
        TimeSpan payAsYoGo,
        Guid userId,
        MembershipLicenseFlags membershipLicenseFlags,
        MembershipBusinessSource membershipBusinessSource)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("必须在事务中调用此方法");
        }

        if (rechargeTimeSpan == default && payAsYoGo == default)
        {
            LogRechargeTimeSpanIsZero(logger, nameof(UserMembershipRechargeReturnAsync), businessOrderId);
            return false;
        }
        var membership = await db.UserMemberships.FirstOrDefaultAsync(x => x.Id == userId);
        if (membership is null)
        {
            return false;
        }

        var membershipLicenseType = ConvertMembershipLicenseType(membershipLicenseFlags, membershipBusinessSource);
        var currentRealExpireDate = membership.ExpireDate.Add(rechargeTimeSpan);

        var now = DateTimeOffset.Now;
        var membershipExpired = MembershipBusinessOrderRepositoryHelper.IsMembershipExpiredAfterRefund(
            currentRealExpireDate,
            membership.PayAsYoGo,
            payAsYoGo,
            now);

        var r = await MembershipChangeAsync(businessOrderId, userId, currentRealExpireDate, membership, membershipLicenseType, membershipExpired, payAsYoGo);
        if (r)
        {
            var flags = await AddMembershipRecordAsync(rechargeTimeSpan, payAsYoGo, userId, currentRealExpireDate, now);
            if (flags)
            {
                flags = await RemoveUserTypeAsync(userId, membershipExpired, UserType.Membership);
                return flags;
            }
        }

        return false;
    }

    async Task<bool> MembershipChangeAsync(
        Guid businessOrderId,
        Guid userId,
        DateTimeOffset currentRealExpireDate,
        UserMembership membership,
        MembershipLicenseFlags membershipLicenseType,
        bool membershipExpired,
        TimeSpan payAsYoGo)
    {
        if (membershipExpired) // 会员过期订阅类型清空
        {
            membership.MemberLicenseFlags = MembershipLicenseFlags.None;
        }
        else
        {
            // 检查当前会员订阅类型是否在当前会员期限内还存在，没有则未用户去除该订阅类型
            var hasFlag = await Entity.AsNoTrackingWithIdentityResolution()
                .AnyAsync(x =>
                x.Id != businessOrderId &&
                x.MemberLicenseType == membershipLicenseType &&
                x.UserId == userId &&
                x.RechargeCompletionTime >= membership.StartDate);

            if (!hasFlag)
            {
                membership.MemberLicenseFlags -= membershipLicenseType;
            }
        }

        membership.ExpireDate = currentRealExpireDate;
        membership.PayAsYoGo += payAsYoGo;

        db.UserMemberships.Update(membership);
        var r = await db.SaveChangesAsync();
        return r > 0;
    }

    async Task<bool> AddMembershipRecordAsync(
        TimeSpan rechargeTimeSpan,
        TimeSpan payAsYoGo,
        Guid userId,
        DateTimeOffset currentRealExpireDate,
        DateTimeOffset? now = null)
    {
        var record = new UserMembershipChangeRecord()
        {
            UserId = userId,
            MembershipChangeDirection = MembershipChangeDirection.Out,
            Value = rechargeTimeSpan,
            Note = "用户退款，撤回",
            CurrentRealExpireDate = currentRealExpireDate,
            CreateTime = now ?? DateTimeOffset.Now,
            PayAsYoGo = payAsYoGo,
        };

        await db.UserMembershipChangeRecords.AddAsync(record);
        var r = await db.SaveChangesAsync();
        return r > 0;
    }

    /// <summary>
    /// 撤回后会员已过期则回收用户会员类型
    /// </summary>
    async Task<bool> RemoveUserTypeAsync(Guid userId, bool membershipExpired, UserType userTypeVal = UserType.Membership)
    {
        if (membershipExpired)
        {
            var exists = await db.Users.AnyAsync(x => x.Id == userId && x.UserType.HasFlag(userTypeVal));
            if (exists)
            {
                var userType = await GetUserQueryWithHasFlag(userId, userTypeVal)
                    .Select(x => (UserType?)x.UserType)
                    .FirstOrDefaultAsync();
                if (userType.HasValue)
                {
                    userType &= ~userTypeVal;
                    var r = await GetUserQueryWithHasFlag(userId, userTypeVal)
                        .ExecuteUpdateAsync(s => s.SetProperty(e => e.UserType, e => userType.Value));
                }
            }
        }

        return true;
    }

    IQueryable<User> GetUserQueryWithHasFlag(Guid userId, UserType userTypeVal = UserType.Membership)
    {
        var query = db.Users
            .Where(x => x.Id == userId && x.UserType.HasFlag(userTypeVal));
        return query;
    }

    /// <summary>
    /// 创建普通会员业务订单
    /// </summary>
    async Task<Order?> CreateMembershipBusinessOrderAsync(
        MembershipBusinessOrder business_order,
        string? orderId = null)
    {
        var timeout = DateTimeOffset.Now.AddMinutes(15);

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(CreateMembershipBusinessOrderCoreAsync);

        async Task<Order?> CreateMembershipBusinessOrderCoreAsync()
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 创建业务订单
                business_order.PaymentStatus = OrderStatus.WaitPay;
                business_order.GoodsRechargeStatus = GoodsRechargeStatus.Waiting;
                Entity.Add(business_order);
                await db.SaveChangesAsync();

                // 创建通用支付订单
                var order = new Order
                {
                    Id = orderId ?? IdGeneratorHelper.GetNextId(),
                    Type = OrderType.GeneralOrder,
                    Source = DevicePlatform2.Windows,
                    Timeout = timeout,
                    Status = OrderStatus.WaitPay,
                    UserId = business_order.UserId,
                    AmountReceivable = business_order.AmountReceivable,
                    BusinessTypeId = OrderBusinessType,
                    Note = business_order.Note,
                    MerchantDeductionAgreementId = null,
                    ChannelPackageId = business_order.ChannelPackageId,
                    BindPCUserId = business_order.BindPCUserId,
                };

                db.Orders.Add(order);
                await db.SaveChangesAsync();

                // 业务订单与通用支付订单关联
                business_order.PaymentStatus = order.Status;
                business_order.OrderId = order.Id;
                order.BusinessOrderId = business_order.Id;
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
            }
            return null;
        }
    }

    internal static class MembershipBusinessOrderRepositoryHelper
    {
        internal static bool IsMembershipExpiredAfterRefund(
            DateTimeOffset currentRealExpireDate,
            TimeSpan currentPayAsYoGo,
            TimeSpan payAsYoGoChange,
            DateTimeOffset now)
        {
            return currentRealExpireDate <= now &&
                (currentPayAsYoGo + payAsYoGoChange) <= TimeSpan.Zero;
        }
    }

    /// <summary>
    /// CDKey 渠道创建会员业务订单
    /// </summary>
    async Task<(bool isOK, UserMembershipChangeRecord? record)> CreateMembershipBusinessOrderByCDKeyAsync(
        MembershipBusinessOrder business_order,
        (Guid bindPCUserId, TimeSpan? bindPCUserExpirePeriod)? bindPCUser = null)
    {
        var now = DateTimeOffset.Now;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(CoreAsync);

        async Task<(bool isOK, UserMembershipChangeRecord? record)> CoreAsync()
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 创建已支付的业务订单
                business_order.GoodsRechargeStatus = GoodsRechargeStatus.Recharged;
                business_order.PaymentStatus = OrderStatus.Paid;
                business_order.PaymentTime = now;
                business_order.RechargeCompletionTime = now;

                await db.MembershipBusinessOrders.AddAsync(business_order);
                var business_order_rowCount = await db.SaveChangesAsync();

                // 会员充值
                var (userMembershipChange_rowCount, record) = await CreateOrUpdateUserMembershipAsync(
                    business_order, now, bindPCUser: bindPCUser);

                // CDKey 可以使用则提交事务
                var membershipProductKeyRecords_rowCount = await db.MembershipProductKeyRecords
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x => x.Id == business_order.ProductKeyRecordId && !x.IsUsed && !x.Disable)
                    .ExecuteUpdateAsync(e =>
                    e.SetProperty(s => s.IsUsed, true)
                    .SetProperty(s => s.UsageTime, DateTimeOffset.Now));
                if (business_order_rowCount > 0 &&
                    userMembershipChange_rowCount > 0 &&
                    membershipProductKeyRecords_rowCount > 0)
                {
                    await transaction.CommitAsync();
                    return (true, record);
                }
                throw new InvalidOperationException();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, null);
            }
        }
    }

    /// <summary>
    /// 用户订阅自动续费会员业务订单
    /// </summary>
    async Task<Order?> CreateMembershipBusinessOrderByAgreementAsync(
        MembershipBusinessOrder business_order,
        PaymentType paymentType,
        string? orderId = null)
    {
        var nextDeductionTime = await db.Set<MerchantDeductionAgreement>()
            .Where(a => a.Id == business_order.MerchantDeductionAgreementId)
            .Select(a => a.NextDeductionTime)
            .FirstOrDefaultAsync(RequestAborted);
        var timeout = nextDeductionTime.Value.Date.AddDays(1);

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(CreateMembershipBusinessOrderByAgreementAsync);

        async Task<Order?> CreateMembershipBusinessOrderByAgreementAsync()
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 添加订单
                var order = new Order
                {
                    Id = orderId ?? IdGeneratorHelper.GetNextId(),
                    Type = OrderType.RenewalOrder,
                    Source = DevicePlatform2.Windows,
                    Timeout = timeout,
                    Status = OrderStatus.WaitPay,
                    UserId = business_order.UserId,
                    AmountReceivable = business_order.AmountReceivable,
                    BusinessTypeId = OrderBusinessType,
                    Note = business_order.Note,
                    MerchantDeductionAgreementId = business_order.MerchantDeductionAgreementId,
                    ChannelPackageId = business_order.ChannelPackageId,
                };
                // 添加订单支付组成
                var composition = new OrderPaymentComposition
                {
                    Order = order,
                    OrderId = order.Id,
                    PaymentMethod = PaymentMethod.Online,
                    PaymentType = paymentType,
                    PaymentAmount = order.AmountReceivable,
                    Note = order.Note,
                    PaymentNumber = "",
                };
                db.OrderPaymentCompositions.Add(composition);
                business_order.Order = order;
                db.MembershipBusinessOrders.Add(business_order);
                await db.SaveChangesAsync();

                order.BusinessOrderId = business_order.Id;
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "创建会员订单错误");
                return null;
            }
        }
    }

    static MembershipLicenseFlags ConvertMembershipLicenseType(MembershipLicenseFlags membershipLicenseType, MembershipBusinessSource businessSource)
    {
        // 只有普通订单才会调整订阅类型，针对连续订阅首次支付
        if (businessSource != MembershipBusinessSource.普通订单)
        {
            return membershipLicenseType;
        }

        // 业务订单订阅类型在用户会员信息的类型调整， 连续订阅只有签约成功后添加
        var finalType = membershipLicenseType switch
        {
            MembershipLicenseFlags.连续包月 => MembershipLicenseFlags.月付,
            MembershipLicenseFlags.连续包季 => MembershipLicenseFlags.季付,
            MembershipLicenseFlags.连续包年 => MembershipLicenseFlags.年付,
            _ => membershipLicenseType
        };

        return finalType;
    }

    #endregion
}

partial class MembershipBusinessOrderRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<MembershipBusinessOrderTableItem>> QueryAsync(
        Guid? id,
        string? goodsName,
        string? goodsNo,
        MembershipLicenseFlags? memberLicenseType,
        string? genericOrderId,
        OrderStatus? paymentStatus,
        DateTimeOffset?[]? paymentTime,
        DateTimeOffset?[]? rechargeCompletionTime,
        MembershipBusinessSource? businessSource,
        GoodsRechargeStatus? goodsRechargeStatus,
        Guid? userId,
        string? cdkey,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.MembershipBusinessOrders
            .AsNoTrackingWithIdentityResolution();

        if (id.HasValue)
            query = query.Where(x => x.Id == id.Value);
        if (!string.IsNullOrEmpty(goodsName))
            query = query.Where(x => x.GoodsName.Contains(goodsName));
        if (!string.IsNullOrEmpty(goodsNo))
            query = query.Where(x => x.GoodsNo.Contains(goodsNo));
        if (memberLicenseType.HasValue)
            query = query.Where(x => x.MemberLicenseType == memberLicenseType.Value);
        if (!string.IsNullOrWhiteSpace(genericOrderId))
            query = query.Where(x => x.OrderId == genericOrderId);
        if (paymentStatus.HasValue)
            query = query.Where(x => x.PaymentStatus == paymentStatus.Value);
        if (businessSource.HasValue)
            query = query.Where(x => x.BusinessSource == businessSource.Value);
        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);

        if (paymentTime != null && paymentTime.Length == 2)
        {
            if (paymentTime[0].HasValue)
                query = query.Where(x => x.PaymentTime >= paymentTime[0]);
            if (paymentTime[1].HasValue)
                query = query.Where(x => x.PaymentTime <= paymentTime[1]);
        }
        if (rechargeCompletionTime != null && rechargeCompletionTime.Length == 2)
        {
            if (rechargeCompletionTime[0].HasValue)
                query = query.Where(x => x.RechargeCompletionTime >= rechargeCompletionTime[0]);
            if (rechargeCompletionTime[1].HasValue)
                query = query.Where(x => x.RechargeCompletionTime <= rechargeCompletionTime[1]);
        }

        if (!string.IsNullOrEmpty(cdkey) && ShortGuid.TryParse(cdkey, out Guid productKeyId))
            query = query.Where(x => x.ProductKeyRecordId == productKeyId);

        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }

        var r = await query
            .ProjectTo<MembershipBusinessOrderTableItem>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }
}

static partial class LogMembershipBusinessOrderRepository
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "充值天数必须不为零，充值 {method} 异常，businessOrderId: {businessOrderId}")]
    internal static partial void LogRechargeTimeSpanIsZero(ILogger logger, string? method, Guid businessOrderId);
}