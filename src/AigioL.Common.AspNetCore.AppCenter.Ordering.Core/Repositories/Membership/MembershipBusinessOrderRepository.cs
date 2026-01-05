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
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Membership;

public partial class MembershipBusinessOrderRepository<TDbContext> :
    Repository<TDbContext, MembershipBusinessOrder, Guid>,
    IMembershipBusinessOrderRepository
    where TDbContext : DbContext, IPaymentDbContext, IIdentityDbContext
{
    readonly ILogger logger;

    public MembershipBusinessOrderRepository(
        ILogger<MembershipBusinessOrderRepository<TDbContext>> logger,
        TDbContext dbContext, IServiceProvider serviceProvider) :
        base(dbContext, serviceProvider)
    {
        this.logger = logger;
    }

    /// <summary>
    /// 订单业务类型（会员业务）
    /// </summary>
    protected virtual int OrderBusinessType => 7;

    #region CreateMembershipBusinessOrder / 创建业务订单

    public async Task<(bool Success, Order? Order)> CreateBusinessOrder(MembershipBusinessOrder business_order, bool isAgreementDeduction = false, PaymentType? paymentType = null)
    {
        // 自动续费订单
        if (isAgreementDeduction)
        {
            if (paymentType is null)
                throw new ArgumentNullException(nameof(paymentType));

            var order = await CreateMembershipBusinessOrderByAgreementAsync(business_order, paymentType!.Value);
            return (order != null, order);
        }
        // CDKey 兑换
        else if (business_order.BusinessSource == MembershipBusinessSource.CDK激活)
        {
            var success = await CreateMembershipBusinessOrderByCDKeyAsync(business_order);
            return (success, null);
        }
        // 普通订单
        else
        {
            var order = await CreateMembershipBusinessOrderAsync(business_order);
            return (order != null, order);
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

                    var userMembershipChangeSuccess = await CreateOrUpdateUserMembershipAsync(business_order, now);

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

    #region Private Methods

    /// <summary>
    /// 创建或更新用户会员信息
    /// </summary>
    /// <param name="business_order"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<bool> CreateOrUpdateUserMembershipAsync(MembershipBusinessOrder business_order, DateTimeOffset now)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("必须在事务中调用此方法");
        }

        var rechargeDays = business_order.RechargeDays;
        var userMembership = await db.UserMemberships.FirstOrDefaultAsync(x => x.Id == business_order.UserId);

        DateTimeOffset currentRealExpireDate;

        var userMembershipChangeSuccess = false;
        if (userMembership == null)
        {
            currentRealExpireDate = now.AddDays(rechargeDays);

            userMembershipChangeSuccess =
                await CreateNewUserMembership() &&
                await ChangeUserType();
        }
        else
        {
            bool changeUserTypeSuccess = false;
            // 之前的会员已经到期
            if (userMembership.ExpireDate <= now)
            {
                currentRealExpireDate = now.AddDays(rechargeDays);
                userMembership.StartDate = now;
                userMembership.ExpireDate = currentRealExpireDate;

                changeUserTypeSuccess = await ChangeUserType();
            }
            else // 在会员期内购买会员
            {
                currentRealExpireDate = userMembership.ExpireDate.AddDays(rechargeDays);
                userMembership.ExpireDate = currentRealExpireDate;
                changeUserTypeSuccess = true;
            }

            var membershipLicenseType = ConvertMembershipLicenseType(business_order.MemberLicenseType, business_order.BusinessSource);
            userMembership.MemberLicenseFlags |= membershipLicenseType;
            db.UserMemberships.Update(userMembership);

            userMembershipChangeSuccess =
                await db.SaveChangesAsync() > 0 &&
                changeUserTypeSuccess;
        }

        if (userMembershipChangeSuccess)
        {
            var userMembershipChangeRecord = new UserMembershipChangeRecord()
            {
                UserId = business_order.UserId,
                MembershipChangeDirection = MembershipChangeDirection.In,
                Days = business_order.RechargeDays,
                Note = business_order.MemberLicenseType.GetDescription(),
                MemberLicenseType = business_order.MemberLicenseType,
                CreateTime = DateTimeOffset.Now,
                CurrentRealExpireDate = currentRealExpireDate,
            };

            db.UserMembershipChangeRecords.Add(userMembershipChangeRecord);
            return await db.SaveChangesAsync() > 0;
        }

        return false;

        // 首次购买会员
        async Task<bool> CreateNewUserMembership()
        {
            userMembership = new UserMembership()
            {
                Id = business_order.UserId,
                StartDate = now,
                ExpireDate = currentRealExpireDate,
                MemberLicenseFlags = business_order.MemberLicenseType,
                FirstMembershipDate = now,
            };
            db.UserMemberships.Add(userMembership);
            return await db.SaveChangesAsync() > 0;
        }

        // 添加会员用户类型，已经是会员则忽略修改
        async Task<bool> ChangeUserType()
        {
            var exists = await db.Users.AsNoTracking().AnyAsync(x => x.Id == business_order.UserId && x.UserType.HasFlag(UserType.Membership));

            if (!exists)
            {
                exists = await db.Users.AsNoTracking().Where(x => x.Id == business_order.UserId)
                   .Where(x => !x.UserType.HasFlag(UserType.Membership))
                   .ExecuteUpdateAsync(s => s.SetProperty(e => e.UserType, e => e.UserType + (int)UserType.Membership)) > 0;
            }

            return exists;
        }
    }

    /// <summary>
    /// 根据业务订单撤销用户会员充值
    /// </summary>
    /// <param name="business_order"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<bool> UserMembershipRechargeReturnAsync(MembershipBusinessOrder business_order)
    {
        if (db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("必须在事务中调用此方法");
        }

        var now = DateTimeOffset.Now;
        var rechargeDays = business_order.RechargeDays;
        if (rechargeDays <= 0)
        {
            logger.LogError($"充值天数必须大于零，充值撤销异常；businessOrderId: {business_order.Id}");
            return false;
        }

        var returnDays = -rechargeDays;

        var membership = await db.UserMemberships.FirstOrDefaultAsync(x => x.Id == business_order.UserId);
        if (membership is null)
            return false;

        var membershipLicenseType = ConvertMembershipLicenseType(business_order.MemberLicenseType, business_order.BusinessSource);
        var currentRealExpireDate = membership.ExpireDate.AddDays(returnDays);
        var membershipExpired = currentRealExpireDate <= now;

        var r = await MembershipChangeAsync();
        if (r)
        {
            return
                await AddMembershipRecordAsync() &&
                await ChangeUserType(membershipExpired);
        }

        return false;

        async Task<bool> MembershipChangeAsync()
        {
            if (membershipExpired) // 会员过期订阅类型清空
            {
                membership.MemberLicenseFlags = MembershipLicenseFlags.None;
            }
            else
            {
                // 检查当前会员订阅类型是否在当前会员期限内还存在，没有则未用户去除该订阅类型
                var hasFlag = await Entity.AsNoTracking()
                    .AnyAsync(x =>
                    x.Id != business_order.Id &&
                    x.MemberLicenseType == membershipLicenseType &&
                    x.UserId == business_order.UserId &&
                    x.RechargeCompletionTime >= membership.StartDate);

                if (!hasFlag)
                    membership.MemberLicenseFlags -= membershipLicenseType;
            }

            membership.ExpireDate = currentRealExpireDate;

            db.UserMemberships.Update(membership);
            return await db.SaveChangesAsync() > 0;
        }

        async Task<bool> AddMembershipRecordAsync()
        {
            var changeRecord = new UserMembershipChangeRecord()
            {
                UserId = membership.Id,
                MembershipChangeDirection = MembershipChangeDirection.Out,
                Days = rechargeDays,
                Note = "用户退款，撤回",
                CurrentRealExpireDate = currentRealExpireDate,
                CreateTime = now
            };

            db.UserMembershipChangeRecords.Add(changeRecord);
            return await db.SaveChangesAsync() > 0;
        }

        // 撤回后会员已过期则回收用户会员类型
        async Task<bool> ChangeUserType(bool membershipExpired)
        {
            var exists = await db.Users.AsNoTracking().AnyAsync(x => x.Id == business_order.UserId && x.UserType.HasFlag(UserType.Membership));

            if (membershipExpired && exists)
            {
                return await db.Users.Where(x => x.Id == business_order.UserId)
                    .Where(x => x.UserType.HasFlag(UserType.Membership))
                    .ExecuteUpdateAsync(s => s.SetProperty(e => e.UserType, e => e.UserType - (int)UserType.Membership)) > 0;
            }

            return true;
        }
    }

    /// <summary>
    /// 创建普通会员业务订单
    /// </summary>
    /// <param name="business_order"></param>
    /// <returns></returns>
    async Task<Order?> CreateMembershipBusinessOrderAsync(MembershipBusinessOrder business_order)
    {
        var timeout = DateTimeOffset.Now.AddMinutes(15);

        return await db.Database.CreateExecutionStrategy().ExecuteAsync(CreateMembershipBusinessOrderCoreAsync);

        async Task<Order?> CreateMembershipBusinessOrderCoreAsync()
        {
            using var transaction = await db.Database.BeginTransactionAsync(RequestAborted);
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
                    Id = IdGeneratorHelper.GetNextId(),
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

    /// <summary>
    /// CDKey 渠道创建会员业务订单
    /// </summary>
    /// <param name="business_order"></param>
    /// <returns></returns>
    async Task<bool> CreateMembershipBusinessOrderByCDKeyAsync(MembershipBusinessOrder business_order)
    {
        var now = DateTimeOffset.Now;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(CoreAsync);

        async Task<bool> CoreAsync()
        {
            using var transaction = await db.Database.BeginTransactionAsync(RequestAborted);
            try
            {
                // 创建已支付的业务订单
                business_order.GoodsRechargeStatus = GoodsRechargeStatus.Recharged;
                business_order.PaymentStatus = OrderStatus.Paid;
                business_order.PaymentTime = now;
                business_order.RechargeCompletionTime = now;

                Entity.Add(business_order);
                var orderAdd = await db.SaveChangesAsync() > 0;

                // 会员充值
                var userMembershipChangeSuccess = await CreateOrUpdateUserMembershipAsync(business_order, now);

                // CDKey 可以使用则提交事务
                var r = await db.MembershipProductKeyRecords
                    .AsNoTrackingWithIdentityResolution()
                    .Where(x => x.Id == business_order.ProductKeyRecordId && !x.IsUsed && !x.Disable)
                    .ExecuteUpdateAsync(e =>
                    e.SetProperty(s => s.IsUsed, true)
                    .SetProperty(s => s.UsageTime, DateTimeOffset.Now)) > 0;
                if (orderAdd && userMembershipChangeSuccess && r)
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

    /// <summary>
    /// 用户订阅自动续费会员业务订单
    /// </summary>
    /// <param name="business_order"></param>
    /// <param name="paymentType"></param>
    /// <returns></returns>
    async Task<Order?> CreateMembershipBusinessOrderByAgreementAsync(MembershipBusinessOrder business_order, PaymentType paymentType)
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
                    Id = IdGeneratorHelper.GetNextId(),
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
                logger.LogError(ex, "创建瓦特会员订单错误");
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