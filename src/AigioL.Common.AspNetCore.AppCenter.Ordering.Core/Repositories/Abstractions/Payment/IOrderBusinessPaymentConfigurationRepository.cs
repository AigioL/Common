using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;

public partial interface IOrderBusinessPaymentConfigurationRepository : IRepository<OrderBusinessPaymentConfiguration, Guid>, IEFRepository
{
}

partial interface IOrderBusinessPaymentConfigurationRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="businessType">业务类型</param>
    /// <param name="paymentMethod">支付方式</param>
    /// <param name="paymentType">支付类型</param>
    /// <param name="disable">是否禁用</param>
    /// <param name="createTime">创建时间</param>
    /// <param name="updateTime">更新时间</param>
    /// <param name="createUser">创建人</param>
    /// <param name="operatorUser">操作人</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="current">当前页码，页码从 1 开始，默认值：<see cref="IPagedModel.DefaultCurrent"/></param>
    /// <param name="pageSize">页大小，如果为 0 必定返回空集合，默认值：<see cref="IPagedModel.DefaultPageSize"/></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedModel<OrderBusinessPaymentConfigurationTableItemModel>> QueryAsync(
        int? businessType,
        PaymentMethod? paymentMethod,
        PaymentType? paymentType,
        bool? disable,
        DateTimeOffset[]? createTime,
        DateTimeOffset[]? updateTime,
        string? createUser,
        string? operatorUser,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);
}