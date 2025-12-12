using AigioL.Common.AspNetCore.AppCenter.Helpers.SnowFlake;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories;

sealed partial class AftersalesBillRepository<TDbContext> :
    Repository<TDbContext, AftersalesBill, Guid>,
    IAftersalesBillRepository
    where TDbContext : DbContext, IOrderingDbContext
{
    public AftersalesBillRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<ApiRsp<(Order? order, AftersalesBillDetailModel? aftersalesBillDetailModel)>> CreateAftersalesBill(
        string orderId,
        string refundReason,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        Order? order = null;
        AftersalesBillDetailModel? aftersalesBillDetailModel = null;
        ApiRsp<(Order? order, AftersalesBillDetailModel? aftersalesBillDetailModel)> r;

        ApiRsp<(Order? order, AftersalesBillDetailModel? aftersalesBillDetailModel)> Error(string errorMessage)
        {
            r = errorMessage;
            r.Content = (order, aftersalesBillDetailModel);
            return r;
        }

        // 检查订单状态
        order = string.IsNullOrWhiteSpace(orderId) ? null : await db.Orders
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.Id == orderId && a.UserId == userId)
            .SingleOrDefaultAsync(cancellationToken);
        if (order == null)
        {
            return Error("找不到要售后的订单");
        }
        if (order.Status is not (OrderStatus.Paid or OrderStatus.Completed))
        {
            return Error("订单不是已付款或已完成状态，无法进行售后");
        }
        if (order.Status == OrderStatus.Refunded)
        {
            return Error("订单已退款，无法继续售后");
        }
        // 检查该订单的售后单状态
        var existingStatus = await db.AftersalesBills
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.OrderId == orderId)
            .Select(a => new
            {
                a.AuditStatus,
                RefundStatus = (RefundStatus?)a.RefundBill!.RefundStatus
            }).ToArrayAsync(cancellationToken);
        if (existingStatus.Any(a => a.RefundStatus == RefundStatus.Refund))
        {
            return Error("订单的其它售后单已通过，无法继续售后");
        }
        // 所有售后单都不通过、或者通过但是退款单被拒绝，这种情况下用户可以再次提交申请
        if (existingStatus.Length > 0 &&
            !existingStatus.All(a =>
                a.AuditStatus == AuditStatus.Rejected ||
                (a.AuditStatus == AuditStatus.Approved && a.RefundStatus == RefundStatus.Refused)))
        {
            return Error("订单已有进行中的售后单");
        }

        var aftersalesBill = new AftersalesBill()
        {
            AftersalesNumber = IdGeneratorHelper.GetNextId(),
            OrderId = order.Id,
            UserId = order.UserId,
            TenantId = order.TenantId,
            RefundAmount = order.AmountReceived,
            AuditStatus = AuditStatus.Pending,
            RefundReason = refundReason,
            CreationTime = DateTimeOffset.Now,
        };
        await db.AftersalesBills.AddAsync(aftersalesBill, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        aftersalesBillDetailModel = await db.AftersalesBills
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.Id == aftersalesBill.Id)
            .Select(ProjectToMapper.AftersalesBillDetailModelExpr)
            .SingleOrDefaultAsync(RequestAborted);

        return (order, aftersalesBillDetailModel);
    }
}

file static class ProjectToMapper
{
    internal static readonly Expression<Func<AftersalesBill, AftersalesBillDetailModel>> AftersalesBillDetailModelExpr = it => new()
    {
        Id = it.Id,
        OrderId = it.OrderId,
        AftersalesNumber = it.AftersalesNumber,
        RefundAmount = it.RefundAmount,
        AuditStatus = it.AuditStatus,
        RefundReason = it.RefundReason ?? "",
        SellerNote = it.SellerNote ?? "",
        CreationTime = it.CreationTime,
        UpdateTime = it.UpdateTime,
        OrderNumber = it.OrderId,
        BusinessType = it.Order.BusinessTypeId,
        Note = it.Order.Note ?? "",
    };
}