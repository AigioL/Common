#pragma warning disable IDE0290 // 使用主构造函数
using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Workers.Abstractions;
using AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;
using AigioL.Common.Models;
using COSXML;
using COSXML.Auth;
using COSXML.Common;
using COSXML.Model;
using COSXML.Model.Object;
using ImageMagick;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.IO;
using System.Net;
using System.Text.Json;
using static COSXML.Model.Tag.ListAllMyBuckets;
namespace AigioL.Common.AspNetCore.AppCenter.COS;

/// <summary>
/// 订单状态订阅
/// </summary>
public static partial class ImageHandleSubscribe
{
    public sealed partial class ImageHandleWorker<TAppSettings> : WorkerBackgroundService
        where TAppSettings : class, IAppSettings
    {
        readonly IServiceProvider serviceProvider;
        readonly HttpClient httpClient;
        readonly TAppSettings appSettings;
        readonly CosXml cosXml;

        public ImageHandleWorker(
            ILogger<ImageHandleWorker<TAppSettings>> logger,
            IOptions<TAppSettings> options,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory,
            IOptions<JsonOptions> jsonOptions,
            IConnection rabbitmqConn,
            IFeishuApiClient feishuApiClient) : base(logger, jsonOptions, rabbitmqConn, feishuApiClient)
        {
            this.serviceProvider = serviceProvider;
            httpClient = httpClientFactory.CreateClient("ImageHandleHttpClient");
            appSettings = options.Value;
            CosXmlConfig config = new CosXmlConfig.Builder()
                .SetRegion(appSettings.ImageHandleCosRegion) // 设置默认的地域, COS 地域的简称请参照 https://cloud.tencent.com/document/product/436/6224
                .Build();
            QCloudCredentialProvider qCloudCredentialProvider =
                new DefaultQCloudCredentialProvider(
                    appSettings.COSSecretId,
                    appSettings.COSSecretKey,
                    appSettings.COSDurationSecond);
            cosXml = new CosXmlServer(config, qCloudCredentialProvider);
        }

        protected override string RoutingKey => CacheKeys.ImageHandleRequest;

        protected override string QueueName => CacheKeys.COSQueueName;


        protected override async Task<ApiRsp> HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            try
            {
                using var container = serviceProvider.CreateScope();
                var imageHandleRequest = JsonSerializer.Deserialize(eventArgs.Body.Span, BasicMinimalApisJsonSerializerContext.Default.ImageHandleRequestModel);
                if (imageHandleRequest == null)
                {
                    return false;
                }
                var imageResponse = await httpClient.GetAsync(imageHandleRequest.ImageUrl);
                switch (imageResponse.StatusCode)
                {
                    case HttpStatusCode.OK:
                        var imageStream = await imageResponse.Content.ReadAsStreamAsync();
                        using (MagickImage image = new MagickImage(imageStream))
                        {
                            image.Format = (MagickFormat)imageHandleRequest.HandleType;
                            image.Resize(imageHandleRequest.Width, imageHandleRequest.Height);
                            image.Quality = imageHandleRequest.Quality; // 设置WebP质量
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                await image.WriteAsync(memoryStream);
                                memoryStream.Position = 0;
                                PutObjectRequest request = new PutObjectRequest(
                                    appSettings.ImageHandleCosBucket,
                                    imageHandleRequest.UrlPath,
                                    memoryStream,
                                    0L,
                                    memoryStream.Length);

                                PutObjectResult result = cosXml.PutObject(request);
                                if (!result.IsSuccessful())
                                {
                                    await feishuApiClient.SendMessageAsync("图片压缩上传腾讯云失败", result.GetResultInfo());
                                }
                            }
                        }
                        break;
                    default:
                        await feishuApiClient.SendMessageAsync("图片下载异常", $"HTTP 状态码：{imageResponse.StatusCode}");
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
                LogErrorHandle(logger, ex);
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
                throw;
            }
        }

        [LoggerMessage(
            Level = LogLevel.Error,
            Message = "图片处理异常")]
        private static partial void LogErrorHandle(ILogger logger, Exception? ex);
    }
}
