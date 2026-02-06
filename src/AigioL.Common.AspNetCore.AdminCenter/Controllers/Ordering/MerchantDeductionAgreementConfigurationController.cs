using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.AddOrEditMerchantDeductionAgreementConfigurationModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.MerchantDeductionAgreementConfigurationTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;

/// <summary>
/// 商家扣款协议配置管理
/// </summary>
public static partial class MerchantDeductionAgreementConfigurationController
{
    const string ControllerName = ControllerConstants.MerchantDeductionAgreementConfiguration;

    public static void MapMerchantDeductionAgreementConfiguration(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/ordering/merchantdeductionagreementconfigurations")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("商家扣款协议配置管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? code = null,
            [FromQuery] string? name = null,
            [FromQuery] string? planId = null,
            [FromQuery] long? period = null,
            [FromQuery] string? periodType = null,
            [FromQuery] decimal? firstAmount = null,
            [FromQuery] decimal? singleAmount = null,
            [FromQuery] PaymentType? platform = null,
            [FromQuery] string? signScene = null,
            [FromQuery] int? businessType = null,
            [FromQuery] string? remarks = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] DateTimeOffset[]? createTime = null,
            [FromQuery] DateTimeOffset[]? updateTime = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var merchantDeductionAgreementConfigurationRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementConfigurationRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await merchantDeductionAgreementConfigurationRepo.QueryAsync(
                code, name, planId,
                period, periodType, firstAmount,
                singleAmount, platform, signScene,
                businessType, remarks, createTime,
                updateTime, orderBy, desc,
                current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询商家扣款协议配置");

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var merchantDeductionAgreementConfigurationRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementConfigurationRepository>();
            BMApiRsp<AddOrEditM?> r = await merchantDeductionAgreementConfigurationRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("获取商家扣款协议配置详情");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            model.Id = default;
            var userId = context.GetBMUserId();
            var merchantDeductionAgreementConfigurationRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementConfigurationRepository>();
            BMApiRsp r = await merchantDeductionAgreementConfigurationRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增商家扣款协议配置项");

        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] AddOrEditM model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }
            var userId = context.GetBMUserId();
            var merchantDeductionAgreementConfigurationRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementConfigurationRepository>();
            BMApiRsp r = await merchantDeductionAgreementConfigurationRepo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改商家扣款协议配置项");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var merchantDeductionAgreementConfigurationRepo = context.RequestServices.GetRequiredService<IMerchantDeductionAgreementConfigurationRepository>();
            var rowCount = await merchantDeductionAgreementConfigurationRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除商家扣款协议配置");
    }
}
