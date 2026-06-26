using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Helpers.SnowFlake;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Payment.Models;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Models;
using MemoryPack;
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
            var r = await repo.GoodsAsync(cacheLock, userId, conn, repo, context.RequestAborted);
            return r;
        }).WithDescription("获取会员商品列表");
        routeGroup.MapPost("create/good/{goodId}/{channelPackageId?}", async (HttpContext context,
            [FromRoute] string goodId,
            [FromRoute] string? channelPackageId) =>
        {
            if (!ShortGuid.TryParse(goodId, out Guid goodIdG) || goodIdG == default)
            {
                return ApiRspCode.BadRequest;
            }
            Guid? channelPackageIdGN = null;
            if (channelPackageId != null)
            {
                var channelPackageService = context.RequestServices.GetService<IChannelPackageService>();
                if (!IChannelPackageService.CheckId(
                    channelPackageService,
                    channelPackageId,
                    out channelPackageIdGN,
                    out ApiRspCode code))
                {
                    return code;
                }
                if (channelPackageIdGN.HasValue)
                {
                    var exists = await channelPackageService.ExistsAsync(channelPackageIdGN.Value, context.RequestAborted);
                    if (!exists)
                    {
                        // 渠道包 Id 不存在
                        return ApiRspCode.NotFound;
                    }
                }
            }
            var userId = context.GetUserIdThrowIfNull();
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MembershipController));
            var repo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            var conn = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var userMembershipService = context.RequestServices.GetRequiredService<IUserMembershipService>();
            var r = await CreateOrderAsync(
                logger, conn, repo,
                userMembershipService, userId, goodIdG,
                channelPackageIdGN);
            return r;
        }).WithDescription("根据会员商品创建会员订单");
        routeGroup.MapPost("create/createagreementsigndeduct", async (HttpContext context,
            [FromBody] MembershipCreateAgreementSignDeductRequest request) =>
        {
            var distributedCache = context.RequestServices.GetRequiredService<IDistributedCache>();
            var r = await CreateAgreementSignDeduct(context, default!, distributedCache, request);
            return r;
        }).WithDescription("商家扣款协议签约并扣款");
    }

    static readonly SemaphoreSlim cacheLock = new(1, 1);

    static async Task<ApiRsp<string?>> CreateOrderAsync(
        ILogger logger,
        IConnectionMultiplexer conn,
        IMembershipGoodsRepository repo,
        IUserMembershipService userMembershipService,
        Guid userId,
        Guid membershipGoodsId,
        Guid? channelPackageId)
    {
        var lockKey = CacheKeys.GetUserRechargeOperationLockKey(userId);
        var r = await conn.LockHandleAsync(lockKey, HandleCoreAsync, errorHandle: ErrorHandleAsync);
        return r;

        Task<ApiRsp<string?>> ErrorHandleAsync(Exception ex)
        {
            logger.LogError(ex, "{userid} create businessOrder error", userId);
            ApiRsp<string?> r = ApiRspCode.InternalServerError;
            return Task.FromResult(r);
        }

        async Task<ApiRsp<string?>> HandleCoreAsync()
        {
            var goods = await repo.FindAsync(membershipGoodsId);

            // 购买不在判断会员类型，上架则视为可购买
            if (goods == null)
            {
                return ApiRsp.Fail<string>("充值商品未找到");
            }

            if (!goods.Enable)
            {
                return ApiRsp.Fail<string>("商品已下架");
            }

            // 直接创建订单
            {
                //var generic_order_id = await userMembershipService.CreateMembershipOrderAsync(
                //    userId,
                //    goods,
                //    channelPackageId);
                //if (!string.IsNullOrWhiteSpace(generic_order_id))
                //{
                //    return ApiRsp.Ok(generic_order_id);
                //}

                //logger.LogTrace("{userId} create businessOrder failed", userId);
                //return ApiRspCode.BadRequest;
            }

            // 延迟创建订单模式
            {
                var cacheKey = IdGeneratorHelper.GetNextId();
                var lazyModel = new LazyCreateMembershipOrderModel()
                {
                    UserId = userId,
                    MembershipGoodsId = membershipGoodsId,
                    ChannelPackageId = channelPackageId,
                };
                var database = conn.GetDatabase(CacheKeys.RedisMessagingDb);
                var lazyModelValue = MemoryPackSerializer.Serialize(lazyModel);
                await database.StringSetAsync($"OrderIdTemp-{cacheKey}", lazyModelValue, TimeSpan.FromMinutes(6d));
                return ApiRsp.Ok(cacheKey);
            }
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
