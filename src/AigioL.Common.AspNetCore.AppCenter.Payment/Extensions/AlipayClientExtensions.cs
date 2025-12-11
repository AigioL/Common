#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Essensoft.Paylink.Alipay;

public static partial class AlipayClientExtensions
{
    /// <summary>
    /// 执行 Alipay API 请求
    /// </summary>
    /// <param name="client">Alipay 客户端</param>
    /// <param name="request">请求对象</param>
    /// <param name="options">配置选项</param>
    /// <returns>响应对象</returns>
    public static async Task<T> RequestExecuteAsync<T>(this IAlipayClient client, IAlipayRequest<T> request, AlipayOptions options) where T : AlipayResponse
    {
        if (IsKeySignature(options))
        {
            return await client.ExecuteAsync(request, options, null);
        }
        else
        {
            return await client.CertificateExecuteAsync(request, options, null);
        }
    }

    /// <summary>
    /// 执行 Alipay 通知请求解析
    /// </summary>
    /// <typeparam name="T">领域对象</typeparam>
    /// <param name="client"></param>
    /// <param name="request">控制器的请求</param>
    /// <param name="options">配置选项</param>
    /// <returns></returns>
    public static async Task<T> RequestExecuteAsync<T>(this IAlipayNotifyClient client, HttpRequest request, AlipayOptions options) where T : AlipayNotify
    {
        if (IsKeySignature(options))
        {
            return await client.ExecuteAsync<T>(request, options);
        }
        else
        {
            return await client.CertificateExecuteAsync<T>(request, options);
        }
    }

    /// <summary>
    /// 接口加签方式配置是“密钥”
    /// </summary>
    /// <param name="options">配置选项</param>
    /// <returns></returns>
    static bool IsKeySignature(AlipayOptions options)
    {
        var r = string.IsNullOrEmpty(options.AppPublicCert)
            || string.IsNullOrEmpty(options.AlipayPublicCert)
            || string.IsNullOrEmpty(options.AlipayRootCert);
        return r;
    }
}
