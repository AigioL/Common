using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment;

public static class TestController
{
    public static void MapPaymentTest(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/test")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapPost("dateAs_ATongUiUMpar", async (HttpContext context,
            [FromQuery] DateTimeOffset debugTime) =>
        {
            await DateAs(context, debugTime);
            return Results.Ok();
        });
#if DEBUG
        routeGroup.MapPost("", async (HttpContext context) =>
        {
            await Transfer(context);
            return Results.Ok();
        });
#endif
    }

    static async Task DateAs(
        HttpContext context,
        DateTimeOffset debugTime)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (!env.IsDevelopment() && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("xunyou-test")))
        //    return;

        //try
        //{
        //    // 获取商家扣款提前天数：默认提前1天
        //    if (!int.TryParse((await keyValuePairRepo.FindAsync("商家扣款提前天数"))?.Value, out int daysInAdvance) || daysInAdvance < 0)
        //        daysInAdvance = 2;

        //    foreach (var agreement in await agreementRepo.GetAgreementsForDeduction(daysInAdvance, debugTime))
        //    {
        //        var xyOrder = new XunYouBusinessOrder
        //        {
        //            OutTradeNo = xunYouService.NewOutTradeNo(),
        //            RechargeDays = xunYouService.GetRechargeDays(agreement.PeriodType, agreement.Period),
        //            AmountReceivable = agreement.SingleAmount,
        //            UserId = agreement.UserId,
        //            Remarks = agreement.Remarks,
        //            MerchantDeductionAgreementId = agreement.Id,
        //            XunYouRechargeProductId = agreement.XunYouMDAExtend?.XunYouRechargeProductId
        //        };

        //        // 创建迅游订单：通用订单、业务订单、支付组成一并创建
        //        var xyOrderResult = await xunYouService.CreateXunYouOrder(xyOrder, agreement.Platform, true);
        //        if (xyOrderResult.State.HasValue)
        //        {
        //            logger.LogError("创建迅游订单错误：{Message}", xyOrderResult.State);
        //            continue;
        //        }
        //        var (_, order, payment) = xyOrderResult.Data;

        //        switch (agreement.Platform)
        //        {
        //            case PaymentType.Alipay:
        //                await aliPayService.ExecuteAgreementDeduction(
        //                    order.OrderNumber,
        //                    order.Remarks ?? string.Empty,
        //                    agreement.SingleAmount,
        //                    agreement.ExtAgreementNo);
        //                break;

        //            case PaymentType.WeChatPay:
        //                await wechatPayService.ExecuteAgreementDeduction(
        //                    order.OrderNumber,
        //                    order.Remarks ?? string.Empty,
        //                    agreement.SingleAmount,
        //                    agreement.ExtAgreementNo);
        //                break;

        //            default:
        //                logger.LogError("不支持的平台");
        //                continue;
        //        }

        //        await Task.Delay(10); // 防止超过调用频率
        //    }
        //}
        //catch (Exception ex)
        //{
        //    logger.LogError(ex, "操作异常");
        //}
    }

#if DEBUG
    static async Task Transfer(
        HttpContext context)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //var transferOrder = new TransferOrder
        //{
        //    UserId = Guid.Parse("7e9ab57c-c67c-4078-bfa2-5dd0b2eac5ec"),
        //    TransferNumber = IdGeneratorHelper.GetNextId(),
        //    Title = "转账测试",
        //    TransferAmount = 0.1M,
        //    UserOpenId = "0117WXxKAQHHjMTf9jGhpIVIw8Y-m3rrtiGxe5BM86TakI5",
        //    UserLoginAccount = null,
        //    TransferStatus = Mobius.Enums.Order.TransferStatus.Pending,
        //};
        //await transferOrderRepo.AddTransferOrder(transferOrder);
        //var result = await aliPayService.Transfer(transferOrder.TransferNumber, transferOrder.TransferAmount, transferOrder.Title, transferOrder.UserOpenId);

        //transferOrder.FailureReason = result.Message;
        //transferOrder.ThirdPartyPlatformNumber = result.ThirdPartyPlatformNumber;
        //transferOrder.TransferStatus = result.TransferStatus;
        //transferOrder.FinishTime = result.FinishTime;

        //await transferOrderRepo.UpdateTransferOrderResult(transferOrder);
    }
#endif
}
