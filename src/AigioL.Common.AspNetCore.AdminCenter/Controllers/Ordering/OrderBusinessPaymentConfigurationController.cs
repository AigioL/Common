using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.OrderBusinessPaymentConfigurationTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;

/// <summary>
/// 业务订单支付配置管理
/// </summary>
public static partial class OrderBusinessPaymentConfigurationController
{
    const string ControllerName = ControllerConstants.OrderBusinessPaymentConfiguration;

    public static void MapOrderBusinessPaymentConfiguration(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/ordering/orderbusinesspaymentconfigurations")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("业务订单支付配置管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] int? businessType = null,
            [FromQuery] PaymentMethod? paymentMethod = null,
            [FromQuery] PaymentType? paymentType = null,
            [FromQuery] bool? disable = null,
            [FromQuery] string? createUser = null,
            [FromQuery] string? operatorUser = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRange("createTime");
            var updateTime = context.GetQueryDateTimeRange("updateTime");
            var orderBusinessPaymentConfigurationRepo = context.RequestServices.GetRequiredService<IOrderBusinessPaymentConfigurationRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await orderBusinessPaymentConfigurationRepo.QueryAsync(
                businessType, paymentMethod, paymentType,
                disable, createTime, updateTime,
                createUser, operatorUser, orderBy,
                desc, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询业务订单支付配置");
    }
}
