using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Controllers;

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
            var userId = context.GetUserIdThrowIfNull();
            var repo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var r = await GoodsAsync(userId, conn, repo, context.RequestAborted);
            return r;
        }).WithDescription("获取会员商品列表");
        routeGroup.MapPost("create", async (HttpContext context,
            [FromBody] MembershipOrderRequest request) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MembershipController));
            var repo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var userMembershipService = context.RequestServices.GetRequiredService<IUserMembershipService>();
            var r = await CreateOrderAsync(logger, conn, repo, userMembershipService, request);
            return r;
        }).WithDescription("创建会员订单");
        routeGroup.MapPost("create/good/{goodId}", async (HttpContext context,
            [FromRoute] string goodId) =>
        {
            if (!ShortGuid.TryParse(goodId, out Guid goodIdG) || goodIdG == default)
            {
                return ApiRspCode.BadRequest;
            }
            var userId = context.GetUserIdThrowIfNull();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MembershipController));
            var repo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var userMembershipService = context.RequestServices.GetRequiredService<IUserMembershipService>();
            var r = await CreateOrderAsync(logger, conn, repo, userMembershipService, new()
            {
                UserId = userId,
                MembershipGoodsId = goodIdG,
            });
            return r;
        }).WithDescription("根据会员商品 Id 创建会员订单");
        routeGroup.MapPost("create/cdkey", async (HttpContext context,
            [FromBody] MembershipCDKeyRequest request) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MembershipController));
            var membershipProductKeyRecordRepo = context.RequestServices.GetRequiredService<IMembershipProductKeyRecordRepository>();
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var userMembershipService = context.RequestServices.GetRequiredService<IUserMembershipService>();
            var r = await CreateByCDKeyAsync(
                logger, conn, membershipProductKeyRecordRepo,
                membershipGoodsRepo, userMembershipService, request);
            return r;
        }).AllowAnonymous()
        .WithDescription("使用 CDKey 兑换会员");
        routeGroup.MapPost("create/CreateAgreementSignDeduct", async (HttpContext context,
            [FromBody] MembershipCreateAgreementSignDeductRequest request) =>
        {
            var distributedCache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var r = await CreateAgreementSignDeduct(context, default!, distributedCache, request);
            return r;
        }).WithDescription("商家扣款协议签约并扣款");
    }

    static readonly SemaphoreSlim cacheLock = new(1, 1);

    /// <summary>
    /// 获取会员商品列表
    /// </summary>
    static async Task<ApiRsp<MembershipGoodsModel[]?>> GoodsAsync(
        Guid userId,
        IConnectionMultiplexer conn,
        IMembershipGoodsRepository repo,
        CancellationToken cancellationToken = default)
    {
        var database = conn.GetDatabase(CacheKeys.RedisMessagingDb);
        var cacheKey = CacheKeys.GetMembershipGoodsCacheKey;

        var goods = await database.GetCacheDataAsync(
            cacheKey,
            repo.GetMembershipGoodsAsync,
            cacheLock,
            cancellationToken: cancellationToken);

        if (goods == null)
            return ApiRspCode.InternalServerError;

        goods = await repo.CheckPriceByUserAsync(userId, goods, cancellationToken);
        return goods;
    }

    static async Task<ApiRsp<string?>> CreateOrderAsync(
        ILogger logger,
        IConnectionMultiplexer conn,
        IMembershipGoodsRepository repo,
        IUserMembershipService userMembershipService,
        MembershipOrderRequest orderRequest)
    {
        var lockKey = CacheKeys.GetSteamRechargeUserOperationLockKey(orderRequest.UserId);
        var r = await conn.LockHandleAsync(lockKey, HandleCoreAsync, errorHandle: ErrorHandleAsync);
        return r;

        Task<ApiRsp<string?>> ErrorHandleAsync(Exception ex)
        {
            logger.LogError(ex, "{userid} create businessOrder error", orderRequest.UserId);
            ApiRsp<string?> r = ApiRspCode.InternalServerError;
            return Task.FromResult(r);
        }

        async Task<ApiRsp<string?>> HandleCoreAsync()
        {
            var goods = await repo.FindAsync(orderRequest.MembershipGoodsId);

            // 支付订单商品类型不能为 CDKey 或 积分兑换
            if (goods == null ||
                goods.MemberLicenseType == MembershipLicenseFlags.CDKey ||
                goods.MemberLicenseType == MembershipLicenseFlags.Points)
            {
                return ApiRsp.Fail<string>("充值商品类型未找到 或充值类型不匹配");
            }

            if (!goods.Enable)
            {
                return ApiRsp.Fail<string>("商品已下架");
            }

            var generic_order_id = await userMembershipService.CreateMembershipOrderAsync(
                orderRequest.UserId,
                goods);
            if (!string.IsNullOrWhiteSpace(generic_order_id))
            {
                return ApiRsp.Ok(generic_order_id);
            }

            logger.LogTrace("{userid} create businessOrder failed", orderRequest.UserId);
            return ApiRspCode.BadRequest;
        }
    }

    static async Task<ApiRsp<bool>> CreateByCDKeyAsync(
        ILogger logger,
        IConnectionMultiplexer conn,
        IMembershipProductKeyRecordRepository membershipProductKeyRecordRepo,
        IMembershipGoodsRepository membershipGoodsRepo,
        IUserMembershipService userMembershipService,
        MembershipCDKeyRequest cdKeyRequest)
    {
        if (!ShortGuid.TryParse(cdKeyRequest.CDKey, out Guid cdKey))
        {
            return "CDKey 不合法";
        }

        var lockKey = CacheKeys.GetSteamRechargeUserOperationLockKey(cdKey);
        var r = await conn.LockHandleAsync(lockKey, HandleCoreAsync, errorHandle: ErrorHandleAsync);
        return r;

        async Task<ApiRsp<bool>> HandleCoreAsync()
        {
            var productKey = await membershipProductKeyRecordRepo.FindAsync(cdKey);
            if (productKey == null || productKey.IsUsed)
            {
                return "CDKey 不存在 或 已被激活";
            }

            var goods = await membershipGoodsRepo.FindAsync(productKey.MembershipGoodsId);
            if (goods == null ||
                (goods.MemberLicenseType != MembershipLicenseFlags.CDKey &&
                goods.MemberLicenseType != MembershipLicenseFlags.Points))
            {
                return "充值商品类型未找到 充值类型不匹配";
            }

            var r = await userMembershipService.CreateMembershipOrderByCDKeyAsync(cdKeyRequest.UserId, productKey, goods);
            if (r)
            {
                return (ApiRspCode.OK, "兑换成功");
            }

            logger.LogTrace("{cdKey}({cdKeyS}) create businessOrder by cdkey failed", cdKey, cdKeyRequest.CDKey);
            return "CDKey 已被使用";
        }

        async Task<ApiRsp<bool>> ErrorHandleAsync(Exception ex)
        {
            await Task.CompletedTask;
            logger.LogError(ex, "{cdKey}({cdKeyS}) create businessOrder by cdkey error", cdKey, cdKeyRequest.CDKey);
            return ApiRspCode.InternalServerError;
        }
    }

    /// <summary>
    /// 商家扣款协议签约并扣款（用户和支付订单创建扣款协议，返回跳转链接）
    /// </summary>
    static async Task<IResult> CreateAgreementSignDeduct(
        HttpContext context,
        string officialUrl,
        IDistributedCache distributedCache,
        MembershipCreateAgreementSignDeductRequest request)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("TODO: 待完善");
        //var paymentServiceStatus = await CacheKeys.GetPaymentServiceStatus(distributedCache);
        //if (!paymentServiceStatus)
        //{
        //    return PaymentHelper.RedirectToWechatPayError(context, officialUrl, ASDRCode.PaymentSystemStopped.Code);
        //}

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
    }

    static bool StringEqualsWithTrim(string? l, string? r)
    {
        if (string.IsNullOrWhiteSpace(l))
        {
            if (string.IsNullOrWhiteSpace(r))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(r))
            {
                return false;
            }
            else
            {
                var l2 = l.AsSpan().Trim();
                var r2 = r.AsSpan().Trim();
                return l2.Equals(r2, StringComparison.InvariantCulture);
            }
        }
    }

    static bool IsParametersSame(
        MerchantDeductionAgreement agreement,
        MembershipCreateAgreementSignDeductRequest request,
        MerchantDeductionAgreementConfiguration configuration)
    {
        return StringEqualsWithTrim(request.Note, agreement.Note) &&
               request.UserId == agreement.UserId &&
               configuration.Id == agreement.ConfigurationId;
    }
}
