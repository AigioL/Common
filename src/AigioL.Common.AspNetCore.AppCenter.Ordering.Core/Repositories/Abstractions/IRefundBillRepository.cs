using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;

public partial interface IRefundBillRepository :
    IRepository<RefundBill, Guid>,
    IEFRepository
{
}

partial interface IRefundBillRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="refundNumber">退款单号</param>
    /// <param name="aftersalesNumber">售后单号</param>
    /// <param name="orderNumber">订单号</param>
    /// <param name="businessType">订单业务类型</param>
    /// <param name="refundAmount">退款金额</param>
    /// <param name="refundMethodType">退款方式类型</param>
    /// <param name="thirdPartyPlatformNumber">第三方平台交易流水号</param>
    /// <param name="refundStatus">退款状态</param>
    /// <param name="refundFailureReason">退款失败原因</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="updateTime">更新时间</param>
    /// <param name="createUser">创建人</param>
    /// <param name="operatorUser">操作人</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="goodsType"></param>
    /// <param name="isSettlement"></param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>RefundBill分页表格查询结果数据</returns>
    Task<PagedModel<RefundBillTableItemModel>> QueryAsync(
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
        CancellationToken cancellationToken = default);

    Task<int> InsertAsync(
        Guid? createUserId,
        AddRefundBillModel model,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> AuditAsync(
        Guid? operatorUserId,
        Guid id,
        EditRefundBillAuditModel model,
        CancellationToken cancellationToken = default);

    Task<OrderRefundInfoModel?> GetOrderRefundInfoByRefundIdAsync(
        Guid refundId,
        CancellationToken cancellationToken = default);

    public sealed record OrderRefundInfoModel(
        OrderStatus Status,
        RefundStatus RefundStatus,
        string OrderNumber,
        string RefundNumber,
        int BusinessTypeId,
        AgreementStatus? MerchantDeductionAgreementStatus,
        string? MerchantDeductionAgreementNo);
}