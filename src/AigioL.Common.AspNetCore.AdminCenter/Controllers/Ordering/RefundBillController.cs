using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using AddM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.AddRefundBillModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.RefundBillTableItemModel;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;

/// <summary>
/// 退款单管理
/// </summary>
public static partial class RefundBillController
{
    const string ControllerName = ControllerConstants.RefundBill;

    public static void MapRefundBill(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/ordering/refundbills")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("退款单管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? userId = null,
            [FromQuery] string? refundNumber = null,
            [FromQuery] string? aftersalesNumber = null,
            [FromQuery] string? orderNumber = null,
            [FromQuery] int? businessType = null,
            [FromQuery] decimal? refundAmount = null,
            [FromQuery] RefundChannelType? refundMethodType = null,
            [FromQuery] string? thirdPartyPlatformNumber = null,
            [FromQuery] RefundStatus? refundStatus = null,
            [FromQuery] string? refundFailureReason = null,
            [FromQuery] string? createUser = null,
            [FromQuery] string? operatorUser = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] string? goodsType = null,
            [FromQuery] bool? isSettlement = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRange("createTime");
            var updateTime = context.GetQueryDateTimeRange("updateTime");
            var refundBillRepo = context.RequestServices.GetRequiredService<IRefundBillRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await refundBillRepo.QueryAsync(
                userId, refundNumber, aftersalesNumber,
                orderNumber, businessType, refundAmount,
                refundMethodType, thirdPartyPlatformNumber, refundStatus,
                refundFailureReason, createTime, updateTime,
                createUser, operatorUser, orderBy,
                desc, goodsType, isSettlement,
                current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询退款单");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddM model) =>
        {
            var userId = context.GetBMUserId();
            var refundBillRepo = context.RequestServices.GetRequiredService<IRefundBillRepository>();
            var rowCount = await refundBillRepo.InsertAsync(userId, model, context.RequestAborted);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增退款单");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var refundBillRepo = context.RequestServices.GetRequiredService<IRefundBillRepository>();
            var rowCount = await refundBillRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除退款单");

        routeGroup.MapPut("{id}/audit", async (HttpContext context,
            [FromRoute] Guid id,
            [FromBody] EditRefundBillAuditModel model) =>
        {
            var userId = context.GetBMUserId();
            var rabbitmqConn = context.RequestServices.GetRequiredService<IConnection>();
            var refundBillRepo = context.RequestServices.GetRequiredService<IRefundBillRepository>();
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            BMApiRsp r = await refundBillRepo.AuditAsync(userId, id, model, context.RequestAborted);
            if (r.IsSuccess)
            {
                if (model.RefundStatus == RefundStatus.Refund)
                {
                    // 忽略错误
                    await PushRefundMessageAsync(refundBillRepo, rabbitmqConn, keyValuePairRepo, id);
                }
            }
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("审核退款单");

        routeGroup.MapPost("{id}/retryrefund", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var rabbitmqConn = context.RequestServices.GetRequiredService<IConnection>();
            var refundBillRepo = context.RequestServices.GetRequiredService<IRefundBillRepository>();
            var keyValuePairRepo = context.RequestServices.GetRequiredService<IKeyValuePairRepository>();
            var r = await PushRefundMessageAsync(refundBillRepo, rabbitmqConn, keyValuePairRepo, id);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("重试发送退款单通知");
    }

    static async Task<ApiRsp> PushRefundMessageAsync(
        IRefundBillRepository refundBillRepo,
        IConnection rabbitmqConn,
        IKeyValuePairRepository keyValuePairRepo,
        Guid id)
    {
        var info = await refundBillRepo.GetOrderRefundInfoByRefundIdAsync(id, CancellationToken.None);
        if (info == null)
        {
            return "未找到退款单的订单信息";
        }

        // 只有已支付 并 退款中或退款失败情况时才可以请求支付平台退款
        var statusValid = info.Status is (OrderStatus.Paid or OrderStatus.Completed) &&
            info.RefundStatus is (RefundStatus.Refunding or RefundStatus.Fail);
        if (!statusValid)
        {
            return "退款单状态错误";
        }


        var message = new OrderRefundMessage(info.OrderNumber, info.RefundNumber);
        await PushOrderRefundRequestMessageAsync(rabbitmqConn, message);

        // 如果业务类型需要退款时需要解约协议
        if (info.MerchantDeductionAgreementStatus == AgreementStatus.Signed &&
            IsRefundAndUnSignMerchantDeductionAgreement(info.BusinessTypeId))
        {
            var kvValue = await keyValuePairRepo.QueryValueAsync($"业务订单退款时自动解约扣款协议_{info.BusinessTypeId}");
            if (kvValue == "1" || (bool.TryParse(kvValue, out var kvValueB) && kvValueB))
            {
                await PushAgreementUnSignRequestMessageAsync(rabbitmqConn, info.MerchantDeductionAgreementNo);
            }
        }

        return true;
    }

    /// <summary>
    /// 判断是否是退款并解约商家扣款协议的业务类型
    /// </summary>
    static bool IsRefundAndUnSignMerchantDeductionAgreement(int businessTypeId)
    {
        return false;
    }

    static async Task PushOrderRefundRequestMessageAsync(
        IConnection rabbitmqConn,
        OrderRefundMessage message)
    {
        using var stream = m.GetStream();
        await JsonSerializer.SerializeAsync(stream, message,
            PaymentMinimalApisJsonSerializerContext.Default.OrderRefundMessage);
        var value = stream.GetMemory();
        await ListRightPushAsync(rabbitmqConn, CacheKeys.PaymentRefundRequest, value, CancellationToken.None);
    }

    /// <summary>
    /// 推送【协议解约申请】通知
    /// </summary>
    /// <param name="rabbitmqConn"></param>
    /// <param name="agreementNo"></param>
    /// <returns></returns>
    static async Task PushAgreementUnSignRequestMessageAsync(
        IConnection rabbitmqConn,
        string agreementNo)
    {
        var value = Encoding.UTF8.GetBytes(agreementNo);
        await ListRightPushAsync(rabbitmqConn, CacheKeys.AgreementUnSignRequest, value, CancellationToken.None);
    }

    static readonly RecyclableMemoryStreamManager m = new();

    const string exchangeName = ""; // 默认交换机

    static async Task ListRightPushAsync(
        IConnection rabbitmqConn,
        string routingKey,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        using var channel = await rabbitmqConn.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.BasicPublishAsync(exchangeName, routingKey, body, cancellationToken);
    }
}
