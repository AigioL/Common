using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories;

sealed partial class OrderRepository<TDbContext> :
    Repository<TDbContext, Order, string>,
    IOrderRepository
    where TDbContext : DbContext, IOrderingDbContext
{
    public OrderRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<OrderDetailModel?> GetOrderInfo(string orderId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return null;
        }

        var query = db.Orders.AsNoTrackingWithIdentityResolution()
            .Where(a => a.UserId == userId)
            .Where(a => a.Id == orderId);

        var orderInfo = await query
            .Select(ProjectToMapper.OrderDetailModelExpr)
            .SingleOrDefaultAsync(cancellationToken);

        if (orderInfo != null)
        {
            //BizOrderInfoDTO? businessOrderInfo = orderInfo.BusinessType switch
            //{
            //    OrderBusinessType.GiftCardRecharge => await db.Set<GiftsCardRechargeBusinessOrder>()
            //        .Where(a => a.GenericOrderId == orderId)
            //        .ProjectTo<GiftsCardRechargeBizOrderInfoDTO>(mapper.ConfigurationProvider)
            //        .SingleOrDefaultAsync(RequestAborted),

            //    OrderBusinessType.SelfServiceGiftCardRecharge => null,

            //    OrderBusinessType.BalanceTransactionRecharge => await db.Set<BalanceTradeRechargeBusinessOrder>()
            //        .Where(a => a.GenericOrderId == orderId)
            //        .ProjectTo<BalanceTradeRechargeBizOrderInfoDTO>(mapper.ConfigurationProvider)
            //        .SingleOrDefaultAsync(RequestAborted),

            //    _ => null,
            //};

            //orderInfo.BusinessOrderInfo = businessOrderInfo;
        }

        return orderInfo;
    }

    public async Task<OrderPayInfoModel?> GetOrderPaymentInfo(string orderId, bool isWaitPay, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            return null;
        }

        var orderStatus = new[] { OrderStatus.WaitPay, OrderStatus.Paid };

        var query = db.Orders.AsNoTrackingWithIdentityResolution()
            .Where(a => a.Id == orderId);

        if (isWaitPay)
            query = query.Where(x => x.Status == OrderStatus.WaitPay);
        else
            query = query.Where(x => orderStatus.Contains(x.Status));

        var orderInfo = await query
            .Select(ProjectToMapper.OrderPayInfoModelExpr)
            .SingleOrDefaultAsync(cancellationToken);

        return orderInfo;
    }

    public async Task<PagedModel<OrderItemInfoModel>> QueryUserOrderListAsync(
        Guid userId,
        long? orderNumber,
        OrderStatus[]? status,
        DateTimeOffset?[]? paymentTime,
        int? businessType,
        string? note,
        DateTimeOffset?[]? createTime,
        int current,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = db.Orders
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreateTime)
            .ThenBy(x => x.Id);

        if (status is { Length: > 0 }) query = query.Where(x => status.Contains(x.Status));
        if (businessType.HasValue) query = query.Where(x => x.BusinessTypeId == businessType.Value);

        if (orderNumber.HasValue) query = query.Where(x => x.OrderNumber.Contains(orderNumber.ToString()!));
        if (!string.IsNullOrEmpty(note)) query = query.Where(x => x.Note!.Contains(note));

        if (paymentTime != null && paymentTime.Length == 2)
        {
            if (paymentTime[0].HasValue)
                query = query.Where(x => x.PaymentTime >= paymentTime[0]);
            if (paymentTime[1].HasValue)
                query = query.Where(x => x.PaymentTime < paymentTime[1]);
        }
        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }

        var r = await query
            .Select(ProjectToMapper.OrderItemInfoModelExpr)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<(ExternalLoginChannel Channel, string? NickName)[]> GetExternalAccountInfoAsync(
        string orderNumber,
        string paymentNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await db.Orders
            .Where(o => o.Id == orderNumber && o.PaymentCompositions!.Any(p => p.PaymentNumber == paymentNumber))
            .SelectMany(o => o.User!.ExternalAccounts!)
            .Select(a => new { a.NickName, a.Type })
            .OrderBy(a => a.Type)
            .Select(static x => new
            {
                x.Type,
                x.NickName,
            })
            .ToArrayAsync(cancellationToken);

        return [.. result.Select(a => (a.Type, a.NickName))];
    }

    public async Task<int> GetUserOrderCountAsync(
        Guid userId,
        OrderStatus[]? status,
        int? businessType,
        CancellationToken cancellationToken = default)
    {
        var query = db.Orders.AsNoTrackingWithIdentityResolution()
            .Where(x => x.UserId == userId);

        if (businessType.HasValue)
            query = query.Where(x => x.BusinessTypeId == businessType.Value);

        if (status is { Length: > 0 })
            query = query.Where(x => status.Contains(x.Status));

        var r = await query.CountAsync(cancellationToken);
        return r;
    }

    public async Task<int?> GetBusinessTypeIdByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var query = db.Orders.AsNoTrackingWithIdentityResolution()
              .Where(x => x.Id == orderNumber)
              .Select(x => x.BusinessTypeId);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task CompleteOrderAsync(string orderNumber)
    {
        var r = await Entity.Where(a => a.Id == orderNumber && a.Status == OrderStatus.Paid)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, OrderStatus.Completed));
        if (r <= 0)
        {
            throw new ApplicationException("完成订单失败，改订单不是已付款状态");
        }
    }
}

file static class ProjectToMapper
{
    internal static readonly Expression<Func<Order, OrderDetailModel>> OrderDetailModelExpr = it => new()
    {
        Id = it.Id,
        OrderNumber = it.Id,
        Type = it.Type,
        Source = it.Source,
        Timeout = it.Timeout,
        Status = it.Status,
        UserId = it.UserId,
        AmountReceivable = it.AmountReceivable,
        AmountReceived = it.AmountReceived,
        PaymentTime = it.PaymentTime,
        BusinessType = it.BusinessTypeId,
        //BusinessId = it.bu
        Note = it.Note,
        CreateTime = it.CreateTime,
    };

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

    internal static readonly Expression<Func<Order, OrderItemInfoModel>> OrderItemInfoModelExpr = it => new()
    {
        Id = it.Id,
        OrderNumber = it.Id,
        Status = it.Status,
        Timeout = it.Timeout,
        PaymentTime = it.PaymentTime,
        BusinessType = it.BusinessTypeId,
        AmountReceivable = it.AmountReceivable,
        Note = it.Note,
        CreateTime = it.CreateTime,
    };
}