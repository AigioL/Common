using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Payment;

sealed partial class PaymentRepository<TDbContext> :
    Repository<TDbContext, OrderPaymentComposition, Guid>,
    IPaymentRepository
    where TDbContext : DbContext, IPaymentDbContext
{
    readonly ILogger logger;

    public PaymentRepository(TDbContext dbContext, IServiceProvider serviceProvider, ILogger<IPaymentRepository> logger) : base(dbContext, serviceProvider)
    {
        this.logger = logger;
    }

    public async Task<OrderBusinessPaymentMethod[]> GetPaymentMethodAsync(int businessType,
        CancellationToken cancellationToken = default)
    {
        var query = db.OrderBusinessPaymentConfigurations
            .AsNoTrackingWithIdentityResolution()
            .Where(a => !a.Disable)
            .Where(a => a.BusinessTypeId == businessType)
            .Select(a => new OrderBusinessPaymentMethod
            {
                PaymentMethod = a.PaymentMethod,
                PaymentType = a.PaymentType,
            })
            .OrderBy(a => a.PaymentMethod)
            .ThenBy(a => a.PaymentType);

        var r = await query.ToArrayAsync(cancellationToken);
        return r;
    }

    public async Task CompletePaymentForOrder(OrderPaymentSuccessInfo orderPaidInfo)
    {
        await db.Database.CreateExecutionStrategy().ExecuteAsync(CompletePaymentForOrderCoreAsync);

        async Task CompletePaymentForOrderCoreAsync()
        {
            try
            {
                using var transaction = await db.Database.BeginTransactionAsync();

                int orderAffected = await db.Orders
                    .Where(x => x.PaymentCompositions!.Any(a => a.Id == orderPaidInfo.PaymentId))
                    .Where(x => x.Status == OrderStatus.WaitPay || x.Status == OrderStatus.Expired || x.Status == OrderStatus.Canceled)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Status, v => OrderStatus.Paid)
                        .SetProperty(x => x.AmountReceived, v => v.AmountReceived + orderPaidInfo.AmountReceived)
                        .SetProperty(x => x.PaymentTime, DateTimeOffset.Now)
                        .SetProperty(x => x.PaymentType, orderPaidInfo.PaymentPlatform)
                        .SetProperty(x => x.UpdateTime, DateTimeOffset.Now));
                if (orderAffected != 1)
                    throw new ApplicationException("更新订单为已支付失败，订单不存在或不处于待支付状态");

                int payCoAffected = await db.OrderPaymentCompositions
                    .Where(a => a.Id == orderPaidInfo.PaymentId)
                    .Where(a => a.PaymentMethod == PaymentMethod.Online &&
                                a.PaymentType == orderPaidInfo.PaymentPlatform &&
                                a.PaymentStatus == PaymentStatus.WaitPay)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.PaymentNumber, x => orderPaidInfo.PaymentPlatformOrderNumber)
                        .SetProperty(x => x.PaymentStatus, PaymentStatus.Paid)
                        .SetProperty(x => x.PaymentTime, DateTimeOffset.Now)
                        .SetProperty(x => x.UpdateTime, DateTimeOffset.Now));
                if (payCoAffected != 1)
                    throw new ApplicationException("更新订单为已支付失败，订单支付组成不存在或不处于待支付状态");

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "订单支付成功通知处理失败，订单号：{PaymentId}，消息内容：{Message}", orderPaidInfo.PaymentId, orderPaidInfo);
                throw;
            }
        }
    }

    public async Task CompleteRefundForOrderAsync(OrderRefundSuccessInfo refundInfo)
    {
        await db.Database.CreateExecutionStrategy().ExecuteAsync(CompleteRefundForOrderCoreAsync);

        async Task CompleteRefundForOrderCoreAsync()
        {
            try
            {
                using var transaction = await db.Database.BeginTransactionAsync();

                int orderAffected = await db.Orders
                    .Where(x => x.AftersalesBills!.Any(a => a.RefundBill!.RefundNumber == refundInfo.RefundNumber))
                    .Where(x => x.Status == OrderStatus.Paid || x.Status == OrderStatus.Completed || x.Status == OrderStatus.Refunded)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Status, v => OrderStatus.Refunded)
                        .SetProperty(x => x.UpdateTime, s => DateTimeOffset.Now));
                if (orderAffected != 1)
                    throw new ApplicationException("更新订单为已退款失败，订单不存在或不处于可退款状态");

                int refundBillAffected = await db.Set<RefundBill>()
                    .Where(a => a.RefundNumber == refundInfo.RefundNumber)
                    .ExecuteUpdateAsync(a => a
                        .SetProperty(b => b.RefundStatus, RefundStatus.Refund)
                        .SetProperty(b => b.RefundFinishTime, DateTimeOffset.Now)
                        );

                int payCoAffected = await db.OrderPaymentCompositions
                    .Where(a => a.Order!.OrderNumber == refundInfo.OrderNumber &&
                                a.PaymentType == refundInfo.PaymentPlatform &&
                                a.PaymentMethod == PaymentMethod.Online &&
                                a.PaymentStatus == PaymentStatus.Paid)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.PaymentStatus, PaymentStatus.Refunded)
                        .SetProperty(x => x.UpdateTime, x => DateTimeOffset.Now));

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "订单退款成功通知处理失败，订单号：{OrderNumber}，消息内容：{Message}", refundInfo.OrderNumber, refundInfo);
                throw;
            }
        }
    }

    public async Task<bool> GetPaymentCompositionStateAsync(string orderId, OrderBusinessPaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        var query = db.OrderPaymentCompositions
            .AsNoTrackingWithIdentityResolution()
            .Where(x =>
                 x.OrderId == orderId &&
                 x.PaymentMethod == method.PaymentMethod &&
                 x.PaymentType == method.PaymentType &&
                 x.PaymentStatus == PaymentStatus.Paid);

        var r = await query.AnyAsync(cancellationToken);
        return r;
    }

    public async Task<OrderPayInfoModel?> GetOrderPaymentInfoAsync(string orderId, bool isWaitPay = false,
        CancellationToken cancellationToken = default)
    {
        var query = db.Orders
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.Id == orderId);
        if (isWaitPay)
            query = query.Where(x => x.Status == OrderStatus.WaitPay);
        var orderInfo = await query
            .Select(ProjectToMapper.OrderPayInfoModelExpr)
            .SingleOrDefaultAsync(cancellationToken);

        return orderInfo;
    }

    public async Task<bool> IsPaymentMethodValidAsync(int type, OrderBusinessPaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        var query = db.OrderBusinessPaymentConfigurations
            .AsNoTrackingWithIdentityResolution()
            .Where(x =>
            x.PaymentMethod == method.PaymentMethod &&
            x.PaymentType == method.PaymentType &&
            x.Disable == false &&
            x.BusinessTypeId == type);

        var any = await query.AnyAsync(cancellationToken);
        return any;
    }

    public async Task<OrderPaymentComposition?> AddOrGetPayMethodAsync(string orderId, decimal amount, OrderBusinessPaymentMethod method)
    {
        var order = await db.OrderPaymentCompositions.FirstOrDefaultAsync(x =>
            x.OrderId == orderId &&
            x.PaymentMethod == method.PaymentMethod &&
            x.PaymentType == method.PaymentType);
        if (order == null)
        {
            order = new OrderPaymentComposition
            {
                OrderId = orderId,
                PaymentMethod = method.PaymentMethod,
                PaymentType = method.PaymentType,
                PaymentAmount = amount,
                PaymentNumber = "",
            };
            db.OrderPaymentCompositions.Add(order);
            var saveChangesResult = await db.SaveChangesAsync();
            if (saveChangesResult == 0)
            {
                return null;
            }
        }
        return order;
    }

    public Task ClosePayment(Guid paymentId)
    {
        // TODO: OrderPaymentComposition 的 PaymentStatus 状态要不要与支付平台的同步？
        //return await db.OrderPaymentCompositions.Where(a => a.PaymentStatus == PaymentStatus.).ToListAsync(RequestAborted);
        return Task.CompletedTask;
    }

    public async Task<(Order Order, OrderPaymentComposition OrderPaymentComposition)?> GetOrderPaymentAsync(string orderNumber, PaymentType paymentType,
        CancellationToken cancellationToken = default)
    {
        var info = await db.OrderPaymentCompositions
            .AsNoTrackingWithIdentityResolution()
            .Include(c => c.Order)
            .ThenInclude(c => c!.MerchantDeductionAgreement)
            .Where(c => c.Order!.OrderNumber == orderNumber && c.PaymentType == paymentType)
            .FirstOrDefaultAsync(cancellationToken);

        if (info == null)
            return null;

        return (info.Order!, info);
    }

    public async Task<OrderPaymentComposition?> GetOnlinePaidCompositionAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var query = db.OrderPaymentCompositions
            .Where(o => o.Order!.OrderNumber == orderNumber)
            .Where(a => a.PaymentStatus == PaymentStatus.Paid && a.PaymentMethod == PaymentMethod.Online);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<RefundBill?> GetRefundBillAsync(string refundNumber, CancellationToken cancellationToken = default)
    {
        var query = db.RefundBills
            .Where(o => o.RefundNumber == refundNumber);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task UpdateRefundBillAsync(string refundNumber, bool refunding, bool refundSuccess, string errorDesc)
    {
        var status = refunding && refundSuccess ?
            RefundStatus.Refund :
            refunding ?
                RefundStatus.Refunding :
                RefundStatus.Fail;

        var query = db.RefundBills
            .Where(a => a.RefundNumber == refundNumber);

        await query.ExecuteUpdateAsync(a => a
            .SetProperty(b => b.RefundStatus, status)
            .SetProperty(b => b.RefundFailureReason, b => errorDesc)
        );
    }
}

file static class ProjectToMapper
{
    internal static readonly Expression<Func<Order, OrderPayInfoModel>> OrderPayInfoModelExpr = it => new()
    {
        Id = it.Id,
        OrderNumber = it.Id,
        Timeout = it.Timeout,
        AmountReceivable = it.AmountReceivable,
        BusinessType = it.BusinessTypeId,
        Note = it.Note,
        Status = it.Status,
        CreateTime = it.CreateTime,
    };
}
