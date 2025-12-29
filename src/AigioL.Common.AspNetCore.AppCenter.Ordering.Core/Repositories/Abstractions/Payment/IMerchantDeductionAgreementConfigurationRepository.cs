using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;

public partial interface IMerchantDeductionAgreementConfigurationRepository : IRepository<MerchantDeductionAgreementConfiguration, Guid>, IEFRepository
{
}

partial interface IMerchantDeductionAgreementConfigurationRepository // 管理后台
{
    /// <summary>
    /// 分页查询表格
    /// </summary>
    /// <param name="code">编号</param>
    /// <param name="name">配置名</param>
    /// <param name="planId">模板 Id</param>
    /// <param name="period">周期数</param>
    /// <param name="periodType">周期类型</param>
    /// <param name="firstAmount">初次扣款金额</param>
    /// <param name="singleAmount">单次扣款金额</param>
    /// <param name="platform">平台类型</param>
    /// <param name="signScene">签约场景码</param>
    /// <param name="businessType">业务类型</param>
    /// <param name="note">备注</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="updateTime">更新时间</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns>商家扣款协议配置分页表格查询结果数据</returns>
    Task<PagedModel<MerchantDeductionAgreementConfigurationTableItemModel>> QueryAsync(
        string? code,
        string? name,
        string? planId,
        long? period,
        string? periodType,
        decimal? firstAmount,
        decimal? singleAmount,
        PaymentType? platform,
        string? signScene,
        int? businessType,
        string? note,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<AddOrEditMerchantDeductionAgreementConfigurationModel?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditMerchantDeductionAgreementConfigurationModel model,
        CancellationToken cancellationToken = default);

    Task<bool> InsertAsync(
        Guid? createUserId,
        AddOrEditMerchantDeductionAgreementConfigurationModel model,
        CancellationToken cancellationToken = default);
}