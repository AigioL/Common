using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Controllers.Payment;

public static class MembershipController
{
    public static void MapPaymentMembership(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "payment/membership")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.ApiControllerBaseAuthorize);

        routeGroup.MapGet("goods", async (HttpContext context) =>
        {
            var r = await GoodsAsync(context);
            return r;
        }).WithDescription("获取会员商品列表");
        routeGroup.MapPost("create", async (HttpContext context,
            [FromBody] MembershipOrderRequest request) =>
        {
            var r = await CreateOrderAsync(context, request);
            return r;
        }).WithDescription("创建会员订单");
        routeGroup.MapPost("create/cdkey", async (HttpContext context,
            [FromBody] MembershipCDKeyRequest request) =>
        {
            var r = await CreateByCDKeyAsync(context, request);
            return r;
        }).AllowAnonymous()
        .WithDescription("使用 CDKey 兑换会员");
        routeGroup.MapPost("create/CreateAgreementSignDeduct", async (HttpContext context,
            [FromBody] MembershipCreateAgreementSignDeductRequest request) =>
        {
            var r = await CreateAgreementSignDeduct(context, request);
            return r;
        }).WithDescription("商家扣款协议签约并扣款");
    }

    /// <summary>
    /// 获取会员商品列表
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    static async Task<ApiRsp<MembershipGoodsModel[]?>> GoodsAsync(
        HttpContext context)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //var userId = HttpContext.GetUserId();
        //if (!userId.HasValue)
        //    return ApiRspCode.Unauthorized;

        //var database = _connection.GetDatabase(CacheKeys.RedisMessagingDb);
        //var cacheKey = CacheKeys.GetMembershipGoodsCacheKey;

        //var goods = await database.GetCacheDataAsync<MembershipGoodsModel[]>(
        //    cacheKey,
        //    _membershipGoodsRepository.GetMembershipGoodsAsync,
        //    _cacheLock);

        //if (goods == null)
        //    return ApiRspCode.InternalServerError;

        //goods = await _membershipGoodsRepository.CheckPriceByUserAsync(userId.Value, goods);

        //return goods;
    }

    static async Task<ApiRsp<Guid?>> CreateOrderAsync(
        HttpContext context,
        MembershipOrderRequest orderRequest)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (!ModelState.IsValid)
        //    return ApiRspCode.RequestModelValidateFail;

        //var lockKey = GetSteamRechargeUserOperationLockKey(orderRequest.UserId);
        //return await _connection.LockHandleAsync(lockKey, HandleCoreAsync, errorHandle: ErrorHandleAsync);

        //async Task<ApiRspImpl<Guid?>> ErrorHandleAsync(Exception ex)
        //{
        //    await Task.CompletedTask;
        //    _logger.LogError(ex, $"{orderRequest.UserId} create businessOrder error");
        //    return ApiRspCode.InternalServerError;
        //}

        //async Task<ApiRspImpl<Guid?>> HandleCoreAsync()
        //{
        //    var goods = await _membershipGoodsRepository.FindAsync(orderRequest.MembershipGoodsId);

        //    // 支付订单商品类型不能为 CDKey 或 积分兑换
        //    if (goods == null ||
        //        goods.MemberLicenseType == MembershipLicenseFlags.CDKey ||
        //        goods.MemberLicenseType == MembershipLicenseFlags.Points)
        //        return "充值商品类型未找到 或充值类型不匹配";

        //    if (!goods.Enable)
        //        return "商品已下架";

        //    var generic_order_id = await _userMembershipService.CreateMembershipOrderAsync(orderRequest.UserId, goods);

        //    if (generic_order_id.HasValue)
        //    {
        //        return generic_order_id;
        //    }
        //    _logger.LogTrace($"{orderRequest.UserId} create businessOrder failed");
        //    return ApiRspCode.BadRequest;
        //}
    }

    static async Task<ApiRsp<bool>> CreateByCDKeyAsync(
        HttpContext context,
        MembershipCDKeyRequest cdKeyRequest)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (!cdKeyRequest.ParsedCDKey.HasValue)
        //    return "CDKey 不合法";

        //var lockKey = GetSteamRechargeUserOperationLockKey(cdKeyRequest.ParsedCDKey.Value);
        //return await _connection.LockHandleAsync(lockKey, HandleCoreAsync, errorHandle: ErrorHandleAsync);

        //async Task<ApiRspImpl<bool>> HandleCoreAsync()
        //{
        //    var productKey = await _productKeyRecordRepository.FindAsync(cdKeyRequest.ParsedCDKey.Value);
        //    if (productKey == null || productKey.IsUsed)
        //        return "CDKey 不存在 或 已被激活";

        //    var goods = await _membershipGoodsRepository.FindAsync(productKey.MembershipGoodsId);
        //    if (goods == null ||
        //        (goods.MemberLicenseType != MembershipLicenseFlags.CDKey &&
        //        goods.MemberLicenseType != MembershipLicenseFlags.Points))
        //        return "充值商品类型未找到 充值类型不匹配";

        //    var r = await _userMembershipService.CreateMembershipOrderByCDKeyAsync(cdKeyRequest.UserId, productKey, goods);
        //    if (r)
        //        return (ApiRspCode.OK, "兑换成功");

        //    _logger.LogTrace($"{cdKeyRequest.ParsedCDKey} create businessOrder by cdkey failed");
        //    return "CDKey 已被使用";
        //}

        //async Task<ApiRspImpl<bool>> ErrorHandleAsync(Exception ex)
        //{
        //    await Task.CompletedTask;
        //    _logger.LogError(ex, $"{cdKeyRequest.ParsedCDKey} create businessOrder by cdkey error");
        //    return ApiRspCode.InternalServerError;
        //}
    }

    /// <summary>
    /// 商家扣款协议签约并扣款（用户和支付订单创建扣款协议，返回跳转链接）
    /// </summary>
    /// <param name="context"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    static async Task<IResult> CreateAgreementSignDeduct(
        HttpContext context,
        MembershipCreateAgreementSignDeductRequest request)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
        //if (!await CacheKeys.GetPaymentServiceStatus(_cache))
        //    return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.PaymentSystemStopped.Code);

        //var userId = request.UserId;
        //var configuration = await _agreementRepo.GetConfiguration(request.ConfigurationCode);
        //if (configuration == null)
        //    return _paymentHelper.RedirectToWechatPayError();

        //if (request.Platform != configuration.Platform)
        //    return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.OrderIsWaitingPayInOtherPlatform.Code);

        //var agreement = await _agreementRepo.GetAgreementAndOrdersByNo(request.AgreementNo);
        //string? agreementNo = agreement?.AgreementNo;
        //decimal? firstAmount = agreement?.FirstAmount;
        //// 重复调用时协议信息会已存在
        //if (agreement != null)
        //{
        //    // 检查协议是否已签约
        //    if (agreement.Status != AgreementStatus.UnSigned)
        //        return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.AgreementAlreadySigned.Code);

        //    // 检查用户是否已签约同业务的协议
        //    if (await _agreementRepo.CheckUserBusinessSigned(userId, configuration.BusinessType))
        //        return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.UserSignedAgreement.Code);

        //    // 协议的首次扣款订单
        //    var order = agreement.Orders?.FirstOrDefault();
        //    if (order == null)
        //        return _paymentHelper.RedirectToWechatPayError();

        //    // 如果参数不一致或订单已过期或订单状态不是待支付，那么返回协议号已存在错误
        //    if (IsParametersSame(agreement, request, configuration))
        //    {
        //        switch (order.Status)
        //        {
        //            case OrderStatus.WaitPay when order.Timeout < DateTimeOffset.Now:
        //            case OrderStatus.Expired:
        //            case OrderStatus.Closed:
        //            case OrderStatus.Canceled:
        //                return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.OrderHaveClosed.Code);

        //            case OrderStatus.Paid:
        //            case OrderStatus.Completed:
        //                return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.OrderHavePaid.Code);
        //            case OrderStatus.Refunded:
        //                return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.AgreementNoAlreadyExists.Code);
        //            default:
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.AgreementNoAlreadyExists.Code);
        //    }
        //}
        //else
        //{
        //    // 检查用户是否已签约同业务的协议
        //    if (await _agreementRepo.CheckUserBusinessSigned(userId, configuration.BusinessType))
        //        return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.UserSignedAgreement.Code);

        //    agreementNo = string.Format(AgreementNoFormat, await _connection.GetAgreementNo(configuration.BusinessType));
        //    // 创建商家扣款协议
        //    (var addSuccess, firstAmount) = await _businessOrderRepository.AddAgreementAndBindOrderAsync(new MerchantDeductionAgreement
        //    {
        //        UserId = userId,
        //        Platform = configuration.Platform,
        //        AgreementNo = agreementNo,
        //        Period = configuration.Period,
        //        PeriodType = configuration.PeriodType,
        //        ExecuteTime = DateTime.Today,
        //        SingleAmount = configuration.SingleAmount,
        //        Remarks = request.Remark,
        //        BusinessType = OrderBusinessType.WattMembership,
        //        ConfigurationId = configuration.Id,
        //        Status = AgreementStatus.UnSigned,
        //    }, request.OrderId);

        //    if (!addSuccess)
        //        return _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.ApiException.Code);
        //}

        //// 缓存请求数据供微信回调后使用
        //var key = RedisKey_AgreementSignDeductRequest_WattMembership + agreementNo;

        //request.AgreementNo = agreementNo;
        //request.FirstAmount = firstAmount!.Value;
        //await _cache.SetV2Async(key, request, 10);

        //// 跳转到微信网页授权链接
        //var returnUrl = new Flurl.Url(_appSettings.ApiUrl).AppendPathSegment("payment/cooperatororder/CreateAgreementSignDeductWithOpenId");
        //return request.Platform switch
        //{
        //    PaymentType.WeChatPay => Redirect(weChatHelper.GetUrlForWechatUserOpenId(returnUrl, agreementNo!)),
        //    PaymentType.Alipay => Redirect(_aliPayServices.GetMiniProgramPayUrl(returnUrl, agreementNo!)),
        //    _ => _paymentHelper.RedirectToWechatPayError(XunYouReturnCode.PaymentTypeNotSupported.Code),
        //};

        //static bool IsParametersSame(
        //    MerchantDeductionAgreement agreement,
        //    MembershipCreateAgreementSignDeductRequest request,
        //    MerchantDeductionAgreementConfiguration configuration)
        //{
        //    return request.Remark == agreement.Remarks &&
        //           request.UserId == agreement.UserId &&
        //           configuration.Id == agreement.ConfigurationId;
        //}
    }
}
