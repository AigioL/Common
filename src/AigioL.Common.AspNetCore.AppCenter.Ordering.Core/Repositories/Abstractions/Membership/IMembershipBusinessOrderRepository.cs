using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;

public partial interface IMembershipBusinessOrderRepository : IRepository<MembershipBusinessOrder, Guid>, IEFRepository
{
}

partial interface IMembershipBusinessOrderRepository // 管理后台
{
    Task<PagedModel<MembershipBusinessOrderTableItem>> QueryAsync(
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
        CancellationToken cancellationToken = default);
}

partial interface IMembershipBusinessOrderRepository
{
    /// <summary>
    /// 创建业务订单 (普通会员、CDKey 兑换、自动续费会员)， if isAgreementDeduction is <see langword="true"/>, paymentType is required
    /// </summary>
    Task<(bool Success, Order? Order)> CreateBusinessOrder(
        MembershipBusinessOrder business_order,
        bool isAgreementDeduction = false,
        PaymentType? paymentType = null,
        string? orderId = null);

    /// <summary>
    /// <see cref="EntityFrameworkQueryableExtensions.FirstOrDefaultAsync{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}}, CancellationToken)"/>
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    Task<MembershipBusinessOrder?> NoTrackingFirstOrDefaultAsync(Expression<Func<MembershipBusinessOrder, bool>> expression);

    /// <summary>
    /// 订单退款成功
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    Task<(bool isSuccess, Guid? userId)> OrderRefunded(string orderId);

    /// <summary>
    /// 订单支付成功
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    Task<(bool isSuccess, Guid? userId)> OrderPaymentSuccess(string orderId);

    /// <summary>
    /// 根据扣款协议 ID 获取业务订单
    /// </summary>
    /// <param name="agreementId"></param>
    /// <returns></returns>
    Task<MembershipBusinessOrder?> GetBusinessOrderByAgreementAsync(Guid agreementId);

    /// <summary>
    /// 添加扣款协议并绑定支付订单和业务订单
    /// </summary>
    /// <param name="agreement"></param>
    /// <param name="orderId"></param>
    /// <returns></returns>
    Task<(bool isSuccess, decimal? firstAmount)> AddAgreementAndBindOrderAsync(
        MerchantDeductionAgreement agreement,
        string orderId);

    /// <summary>
    /// 创建或更新用户会员信息
    /// </summary>
    Task<bool> CreateOrUpdateUserMembershipAsync(
       Guid businessOrderId,
       TimeSpan rechargeTimeSpan,
       Guid userId,
       MembershipLicenseFlags membershipLicenseFlags,
       MembershipBusinessSource membershipBusinessSource,
       DateTimeOffset? now = null);
}