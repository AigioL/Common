using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Workers;

/// <summary>
/// 会员业务订单订阅支付订单消息
/// </summary>
public abstract partial class MembershipOrderSubscribe : WorkerBackgroundService
{
    protected readonly IServiceProvider serviceProvider;
    protected readonly IOrderBusinessTypeService orderBusinessTypeService;

    protected MembershipOrderSubscribe(
        ILogger logger,
        IServiceProvider serviceProvider,
        IOptions<JsonOptions> jsonOptions,
        IOrderBusinessTypeService orderBusinessTypeService,
        IConnection rabbitmqConn,
        IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
    {
        this.serviceProvider = serviceProvider;
        this.orderBusinessTypeService = orderBusinessTypeService;
    }

    /// <summary>
    /// 用户签约成功订阅
    /// </summary>
    public sealed partial class AgreementSignWorker : MembershipOrderSubscribe
    {
        const string moduleName = "用户签约成功";

        public AgreementSignWorker(
            ILogger<AgreementSignWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<JsonOptions> jsonOptions,
            IOrderBusinessTypeService orderBusinessTypeService,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, serviceProvider, jsonOptions, orderBusinessTypeService, rabbitmqConn, feishuApiClient)
        {
        }

        protected override string RoutingKey => CacheKeys.AgreementSign_Membership;

        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var agreementNo = Encoding.UTF8.GetString(eventArgs.Body.Span);
            await HandleCoreAsync(serviceProvider, logger, (service) =>
                service.SignMerchantDeductionSuccessHandleAsync(agreementNo),
                moduleName,
                agreementNo);
            return true; // 返回成功移除消息队列，错误已由 HandleCoreAsync 处理
        }
    }

    /// <summary>
    /// 用户解约成功订阅
    /// </summary>
    public sealed partial class AgreementUnSignWorker : MembershipOrderSubscribe
    {
        const string moduleName = "用户解约成功";

        public AgreementUnSignWorker(
            ILogger<AgreementUnSignWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<JsonOptions> jsonOptions,
            IOrderBusinessTypeService orderBusinessTypeService,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, serviceProvider, jsonOptions, orderBusinessTypeService, rabbitmqConn, feishuApiClient)
        {
        }

        protected override string RoutingKey => CacheKeys.AgreementUnSign_Membership;

        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var agreementNo = Encoding.UTF8.GetString(eventArgs.Body.Span);
            await HandleCoreAsync(serviceProvider, logger, (service) =>
                service.UnSignMerchantDeductionSuccessHandleAsync(agreementNo),
                moduleName,
                agreementNo);
            return true; // 返回成功移除消息队列，错误已由 HandleCoreAsync 处理
        }
    }

    /// <summary>
    /// 订单支付成功订阅
    /// </summary>
    public sealed partial class PaymentSuccessWorker : MembershipOrderSubscribe
    {
        const string moduleName = "支付成功";

        public PaymentSuccessWorker(
            ILogger<PaymentSuccessWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<JsonOptions> jsonOptions,
            IOrderBusinessTypeService orderBusinessTypeService,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, serviceProvider, jsonOptions, orderBusinessTypeService, rabbitmqConn, feishuApiClient)
        {
        }

        protected override string RoutingKey => CacheKeys.GetPaymentSuccessMessageQueueKeyByBusinessType(orderBusinessTypeService.Membership);

        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var orderId = Encoding.UTF8.GetString(eventArgs.Body.Span);
            await HandleCoreAsync(serviceProvider, logger, (service) =>
                service.OrderPaymentSuccessHandleAsync(orderId),
                moduleName,
                orderId);
            return true; // 返回成功移除消息队列，错误已由 HandleCoreAsync 处理
        }
    }

    /// <summary>
    /// 已支付订单创建售后终止业务订阅
    /// </summary>
    public sealed partial class PaidOrderCancelWorker : MembershipOrderSubscribe
    {
        const string moduleName = "已支付订单取消";

        public PaidOrderCancelWorker(
            ILogger<PaidOrderCancelWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<JsonOptions> jsonOptions,
            IOrderBusinessTypeService orderBusinessTypeService,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, serviceProvider, jsonOptions, orderBusinessTypeService, rabbitmqConn, feishuApiClient)
        {
        }

        protected override string RoutingKey => CacheKeys.GetOrderUserRequestRefundMessageQueueKeyByBusinessType(orderBusinessTypeService.Membership);

        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var orderId = Encoding.UTF8.GetString(eventArgs.Body.Span);
            await HandleCoreAsync(serviceProvider, logger, (service) =>
                service.OrderPaymentCancelHandleAsync(orderId),
                moduleName,
                orderId);
            return true; // 返回成功移除消息队列，错误已由 HandleCoreAsync 处理
        }
    }

    /// <summary>
    /// 订单退款成功订阅
    /// </summary>
    public sealed partial class PaymentRefundedWorker : MembershipOrderSubscribe
    {
        const string moduleName = "退款成功";

        public PaymentRefundedWorker(
            ILogger<PaymentRefundedWorker> logger,
            IServiceProvider serviceProvider,
            IOptions<JsonOptions> jsonOptions,
            IOrderBusinessTypeService orderBusinessTypeService,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, serviceProvider, jsonOptions, orderBusinessTypeService, rabbitmqConn, feishuApiClient)
        {
        }

        protected override string RoutingKey => CacheKeys.GetOrderRefundedMessageQueueKeyByBusinessType(orderBusinessTypeService.Membership);

        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var orderId = Encoding.UTF8.GetString(eventArgs.Body.Span);
            await HandleCoreAsync(serviceProvider, logger, (service) =>
                service.OrderPaymentRefundedHandleAsync(orderId),
                moduleName,
                orderId);
            return true; // 返回成功移除消息队列，错误已由 HandleCoreAsync 处理
        }
    }

    protected async Task HandleCoreAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        Func<IUserMembershipService, Task<bool>> handleAsync,
        string moduleName,
        string args)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IUserMembershipService>();

            if (!await handleAsync(service))
            {
                LogFailed(logger, moduleName, args);
                var workerName = WorkerName;
                var message = $"会员业务订单通用订单 {moduleName} 订阅，业务订单状态修改失败，参数：{args}";
                await OnHandleFailAsync(workerName, message);
                return;
            }
        }
        catch (Exception ex)
        {
            var workerName = WorkerName;
            var message = $"会员业务订单通用订单 {moduleName} 订阅，执行异常，参数：{args}";
            await OnHandleFailAsync(workerName, message);
            LogError(logger, ex, moduleName, args);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "会员业务订单通用订单 {moduleName} 订阅，业务订单状态修改失败，参数：{args}")]
    protected static partial void LogFailed(
        ILogger logger,
        string? moduleName,
        string? args);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "会员业务订单通用订单 {moduleName} 订阅，执行异常，参数：{args}")]
    protected static partial void LogError(
        ILogger logger,
        Exception? exception,
        string? moduleName,
        string? args);
}
