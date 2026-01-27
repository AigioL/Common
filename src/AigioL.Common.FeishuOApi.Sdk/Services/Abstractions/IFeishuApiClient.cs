using AigioL.Common.Models;

namespace AigioL.Common.FeishuOApi.Sdk.Services.Abstractions;

/// <summary>
/// 飞书开放平台 WebApi 调用接口
/// </summary>
public partial interface IFeishuApiClient
{
    /// <summary>
    /// 发送富文本消息
    /// <para>https://open.feishu.cn/document/client-docs/bot-v3/add-custom-bot#f62e72d5</para>
    /// </summary>
    Task<ApiRsp> SendMessageAsync(
        string? title,
        string? text,
        CancellationToken cancellationToken = default);

    Task<ApiRsp> SendMessageCoreAsync(
        string? hookId,
        string? serverTag,
        string? title,
        string? text,
        CancellationToken cancellationToken = default);
}