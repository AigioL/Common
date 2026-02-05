using AigioL.Common.AspNetCore.AppCenter.Helpers.SnowFlake;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        Guid? userId,
        decimal? refundAmount = null,
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
        if (string.IsNullOrWhiteSpace(orderId))
        {
            order = null;
        }
        else
        {
            var query = db.Orders.AsNoTrackingWithIdentityResolution()
                .Where(a => a.Id == orderId);
            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }
            order = await query.SingleOrDefaultAsync(cancellationToken);
        }
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

        if (!refundAmount.HasValue)
        {
            refundAmount = order.AmountReceived;
        }
        else if (refundAmount.Value < 0 || refundAmount.Value > order.AmountReceived)
        {
            return Error("退款金额不能超过订单实收金额");
        }

        var aftersalesBill = new AftersalesBill()
        {
            AftersalesNumber = IdGeneratorHelper.GetNextId(),
            OrderId = order.Id,
            UserId = order.UserId,
            TenantId = order.TenantId,
            RefundAmount = refundAmount.Value,
            AuditStatus = AuditStatus.Pending,
            RefundReason = refundReason,
            CreateTime = DateTimeOffset.Now,
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

partial class AftersalesBillRepository<TDbContext>
{
    public async Task<PagedModel<AftersalesBillTableItem>> QueryAsync(
        string? orderNumber,
        string? aftersalesNumber,
        int? businessType,
        Guid? userId,
        decimal? refundAmount,
        AuditStatus[]? auditStatus,
        string? refundReason,
        string? sellerNote,
        DateTimeOffset?[]? createTime,
        DateTimeOffset?[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<AftersalesBill> query = db.AftersalesBills
            .AsNoTrackingWithIdentityResolution()
            .OrderBy(x => x.AuditStatus == AuditStatus.Approved || x.AuditStatus == AuditStatus.Rejected);
        if (!string.IsNullOrEmpty(orderNumber))
            query = query.Where(x => x.Order!.Id.Contains(orderNumber));
        if (!string.IsNullOrEmpty(aftersalesNumber))
            query = query.Where(x => x.AftersalesNumber.Contains(aftersalesNumber));
        if (businessType.HasValue) query = query.Where(x => x.Order!.BusinessTypeId == businessType.Value);
        if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);
        if (refundAmount.HasValue)
            query = query.Where(x => x.RefundAmount == refundAmount.Value);
        if (auditStatus is { Length: > 0 }) query = query.Where(x => auditStatus.Contains(x.AuditStatus));
        if (!string.IsNullOrEmpty(refundReason))
            query = query.Where(x => x.RefundReason!.Contains(refundReason));
        if (!string.IsNullOrEmpty(sellerNote))
            query = query.Where(x => x.SellerNote!.Contains(sellerNote));
        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }
        if (updateTime != null && updateTime.Length == 2)
        {
            if (updateTime[0].HasValue)
                query = query.Where(x => x.UpdateTime >= updateTime[0]);
            if (updateTime[1].HasValue)
                query = query.Where(x => x.UpdateTime < updateTime[1]);
        }

        if (!string.IsNullOrEmpty(createUser))
            if (ShortGuid.TryParse(createUser, out Guid createUserId))
                query = query.Where(x => x.CreateUser!.Id == createUserId);
            else
                query = query.Where(x => x.CreateUser!.NickName!.Contains(createUser));
        if (!string.IsNullOrEmpty(operatorUser))
            if (ShortGuid.TryParse(operatorUser, out Guid operatorUserId))
                query = query.Where(x => x.OperatorUser!.Id == operatorUserId);
            else
                query = query.Where(x => x.OperatorUser!.NickName!.Contains(operatorUser));
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime).ThenBy(x => x.Id);
        }

        var r = await query.ProjectTo<AftersalesBillTableItem>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<ApiRsp> AuditAsync(
        Guid? operatorUserId,
        Guid id,
        EditAftersalesBillAuditModel model)
    {
        var status = model.AuditStatus;
        var sellerNote = model.SellerNote;

        if (status != AuditStatus.Approved && status != AuditStatus.Rejected)
        {
            return "售后单审批只能通过或拒绝";
        }

        var whereBase = db.AftersalesBills.Where(a => a.Id == id);
        var whereAuditing = whereBase.Where(a => a.AuditStatus == AuditStatus.Pending || a.AuditStatus == AuditStatus.InProgress);

        // SteamBot 相关订单的售后单需要检查 SteamBot 是否终止成功 <see cref="IRechargeBusinessOrderRepository.CancelPaidOrder(Guid)"/>
        //if (await whereBase.AnyAsync(a =>
        //    a.Order!.BusinessType == OrderBusinessType.BalanceTransactionRecharge ||
        //    a.Order.BusinessType == OrderBusinessType.GiftCardRecharge ||
        //    a.Order.BusinessType == OrderBusinessType.PointRewardRecharge ||
        //    a.Order.BusinessType == OrderBusinessType.PointRewardUserRecharge))
        //{
        //    var canceled = await whereBase.AnyAsync(a => a.Order!.Status == OrderStatus.Canceled);
        //    if (!canceled)
        //        return "该售后单对应的订单业务没有终止成功";
        //}

        var refunded = await whereBase.AnyAsync(a => a.Order!.Status == OrderStatus.Refunded);
        if (refunded)
        {
            return "该售后单对应的订单已退款";
        }

        if (status == AuditStatus.Approved)
        {
            var approved = await whereBase.AnyAsync(a => a.Order!.AftersalesBills!.Any(o => o.AuditStatus == AuditStatus.Approved && o.RefundBill!.RefundStatus != RefundStatus.Refused));
            if (approved)
            {
                return "该售后单对应的订单已有其它审批通过售后单";
            }
        }

        if (status == AuditStatus.Rejected)
        {
            var r = await whereAuditing.ExecuteUpdateAsync(s => s.SetProperty(a => a.AuditStatus, status).SetProperty(a => a.SellerNote, sellerNote));
            if (r == 0)
            {
                return "售后单审批失败，状态发生了变化";
            }

            return true;
        }

        // 进行退款业务逻辑
        var aftersalesBill = await whereAuditing.Include(a => a.Order).ThenInclude(a => a.PaymentCompositions).SingleOrDefaultAsync();
        if (aftersalesBill == null)
        {
            return "售后单审批失败，状态发生了变化";
        }

        // 修改售后单审批状态
        aftersalesBill.AuditStatus = status;
        aftersalesBill.SellerNote = sellerNote;

        if (status == AuditStatus.Approved)
        {
            // 目前只考虑退在线支付的
            var paymentCompositions = aftersalesBill.Order!.PaymentCompositions!.Where(
                a => a.PaymentStatus == PaymentStatus.Paid && a.PaymentMethod == PaymentMethod.Online);
            foreach (var paymentComposition in paymentCompositions)
            {
                // 添加退款单
                RefundBill refundBill = new()
                {
                    RefundNumber = IdGeneratorHelper.GetNextId(),
                    AftersalesBillId = aftersalesBill.Id,
                    TenantId = aftersalesBill.TenantId,
                    UserId = aftersalesBill.UserId,
                    RefundAmount = aftersalesBill.RefundAmount,
                    RefundMethodType = RefundChannelType.OriginalWay,
                    RefundStatus = RefundStatus.NoRefund,
                    ThirdPartyPlatformNumber = paymentComposition.PaymentNumber,
                };
                await db.RefundBills.AddAsync(refundBill);
            }
            await db.SaveChangesAsync();
        }

        return true;
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
        CreateTime = it.CreateTime,
        UpdateTime = it.UpdateTime,
        OrderNumber = it.OrderId,
        BusinessType = it.Order.BusinessTypeId,
        Note = it.Order.Note ?? "",
    };
}