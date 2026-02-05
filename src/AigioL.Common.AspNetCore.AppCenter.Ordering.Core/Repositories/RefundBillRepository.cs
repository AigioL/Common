using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using static AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.IRefundBillRepository;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories;

sealed partial class RefundBillRepository<TDbContext> :
    Repository<TDbContext, RefundBill, Guid>,
    IRefundBillRepository
    where TDbContext : DbContext, IOrderingDbContext
{
    public RefundBillRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class RefundBillRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<RefundBillTableItemModel>> QueryAsync(
        Guid? userId,
        string? refundNumber,
        string? aftersalesNumber,
        string? orderNumber,
        int? businessType,
        decimal? refundAmount,
        RefundChannelType? refundMethodType,
        string? thirdPartyPlatformNumber,
        RefundStatus? refundStatus,
        string? refundFailureReason,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        string? goodsType,
        bool? isSettlement,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {

        var mapper = serviceProvider.GetRequiredService<IMapper>();
        IQueryable<RefundBill> query = db.RefundBills
            .Include(x => x.User)
            .Include(x => x.AftersalesBill)
            .ThenInclude(x => x.Order)
            .AsNoTrackingWithIdentityResolution();

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId);
        if (!string.IsNullOrEmpty(refundNumber))
            query = query.Where(x => x.RefundNumber.Contains(refundNumber));
        if (!string.IsNullOrEmpty(aftersalesNumber))
            query = query.Where(x => x.AftersalesBill!.AftersalesNumber.Contains(aftersalesNumber));
        if (!string.IsNullOrEmpty(orderNumber))
            query = query.Where(x => x.AftersalesBill!.Order!.Id.Contains(orderNumber));
        if (businessType.HasValue)
            query = query.Where(x => x.AftersalesBill!.Order!.BusinessTypeId == businessType.Value);
        if (!string.IsNullOrEmpty(goodsType))
            query = query.Where(x => x.AftersalesBill!.Order!.Note == goodsType);
        if (refundAmount.HasValue)
            query = query.Where(x => x.RefundAmount == refundAmount.Value);
        if (refundMethodType.HasValue)
            query = query.Where(x => x.RefundMethodType == refundMethodType.Value);
        if (!string.IsNullOrEmpty(thirdPartyPlatformNumber))
            query = query.Where(x => x.ThirdPartyPlatformNumber!.Contains(thirdPartyPlatformNumber));
        if (refundStatus.HasValue)
            query = query.Where(x => x.RefundStatus == refundStatus.Value);
        if (!string.IsNullOrEmpty(refundFailureReason))
            query = query.Where(x => x.RefundFailureReason!.Contains(refundFailureReason));
        if (createTime != null)
            query = createTime.Length switch
            {
                1 => query.Where(x => x.CreateTime >= createTime[0]),
                2 => query.Where(x => x.CreateTime >= createTime[0] && x.CreateTime <= createTime[1]),
                _ => query,
            };
        if (updateTime != null)
            query = updateTime.Length switch
            {
                1 => query.Where(x => x.UpdateTime >= updateTime[0]),
                2 => query.Where(x => x.UpdateTime >= updateTime[0] && x.UpdateTime <= updateTime[1]),
                _ => query,
            };
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
        if (isSettlement.HasValue && createTime != null && createTime.Length >= 1)
        {
            if (isSettlement == true)
            {
                var lastMonthstime = createTime[0].AddDays(1 - createTime[0].Day).AddMonths(-1);
                var lastMonthetime = createTime[0].AddDays(1 - createTime[0].Day);
                query = query.Where(x => x.AftersalesBill!.Order.PaymentTime >= lastMonthstime && x.AftersalesBill.Order.PaymentTime < lastMonthetime);
            }
            else if (createTime.Length >= 2)
            {
                query = query.Where(x => x.AftersalesBill!.Order.PaymentTime >= createTime[0] && x.AftersalesBill.Order.PaymentTime < createTime[1]);
            }
        }
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.CreateTime);
        }

        var r = await query.ProjectTo<RefundBillTableItemModel>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<int> InsertAsync(
        Guid? createUserId,
        AddRefundBillModel model,
        CancellationToken cancellationToken = default)
    {
        RefundBill entity = new()
        {
            RefundNumber = model.RefundNumber,
            RefundAmount = model.RefundAmount,
            RefundMethodType = model.RefundMethodType,
            ThirdPartyPlatformNumber = model.ThirdPartyPlatformNumber,
            RefundStatus = model.RefundStatus,
            RefundFailureReason = model.RefundFailureReason,
            CreateUserId = createUserId,
        };
        await db.RefundBills.AddAsync(entity, cancellationToken);
        var r = await db.SaveChangesAsync(CancellationToken.None);
        return r;
    }

    public async Task<ApiRsp> AuditAsync(
        Guid? operatorUserId,
        Guid id,
        EditRefundBillAuditModel model,
        CancellationToken cancellationToken = default)
    {
        var status = model.RefundStatus;
        var whereBase = db.RefundBills.Where(a => a.Id == id);
        var whereAuditing = whereBase.Where(a => a.RefundStatus == RefundStatus.NoRefund);

        if (status != RefundStatus.Refund && status != RefundStatus.Refused)
        {
            return "退款单审批只能通过或拒绝";
        }
        var refunded = await whereBase.AnyAsync(a => a.AftersalesBill!.Order!.Status == OrderStatus.Refunded, cancellationToken);
        if (refunded)
        {
            return "该退款单对应的订单已退款";
        }
        var approved = await whereBase.AnyAsync(a =>
            a.AftersalesBill!.Order!.AftersalesBills!.Any(o => o.RefundBill!.RefundStatus == RefundStatus.Refund),
            cancellationToken);
        if (approved)
        {
            return "该退款单对应的订单已有其它审批通过退款单";
        }

        if (status == RefundStatus.Refused)
        {
            var r = await whereAuditing.ExecuteUpdateAsync(s => s.SetProperty(a => a.RefundStatus, RefundStatus.Refused), CancellationToken.None);
            if (r == 0)
            {
                return "退款单审批失败，状态发生了变化";
            }
            return true;
        }
        if (status == RefundStatus.Refund)
        {
            //if (model.ForceRefund is not true)
            //{
            //    if (await CheckLimitByForthwithRaffle(id))
            //    {
            //        return "用户参与过会员即时抽奖，无法退款";
            //    }
            //}
            var r = await whereAuditing.ExecuteUpdateAsync(s => s.SetProperty(a => a.RefundStatus, RefundStatus.Refunding), CancellationToken.None);
            if (r == 0)
            {
                return "退款单审批失败，状态发生了变化";
            }
            return true;
        }
        return "退款单审批失败";
    }

    public async Task<OrderRefundInfoModel?> GetOrderRefundInfoByRefundIdAsync(
        Guid refundId,
        CancellationToken cancellationToken = default)
    {
        var query = db.RefundBills
            .Include(x => x.AftersalesBill!)
                .ThenInclude(x => x.Order)
                    .ThenInclude(x => x.MerchantDeductionAgreement)
            .Where(x => x.Id == refundId)
            .Select(x => (x.AftersalesBill == null || x.AftersalesBill.Order == null) ? default : new OrderRefundInfoModel(
                x.AftersalesBill.Order.Status,
                x.RefundStatus,
                x.AftersalesBill.Order.Id,
                x.RefundNumber,
                x.AftersalesBill.Order.BusinessTypeId,
                x.AftersalesBill.Order.MerchantDeductionAgreement == null ? default : x.AftersalesBill.Order.MerchantDeductionAgreement.Status,
                x.AftersalesBill.Order.MerchantDeductionAgreement == null ? default : x.AftersalesBill.Order.MerchantDeductionAgreement.AgreementNo));

        var r = await query.SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(r?.OrderNumber))
            return null;
        return r;
    }
}