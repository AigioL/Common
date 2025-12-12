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
        CreationTime = it.CreationTime,
    };
}
