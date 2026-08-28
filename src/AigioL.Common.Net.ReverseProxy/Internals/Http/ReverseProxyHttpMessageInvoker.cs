using AigioL.Common.Net.ReverseProxy.Internals.Configuration;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Internals.Http;

/// <summary>
/// 反向代理的 <see cref="HttpMessageInvoker"/>
/// </summary>
sealed class ReverseProxyHttpMessageInvoker(HttpMessageHandler handler, bool disposeHandler, IReverseProxyConfig reverseProxyConfig) : HttpMessageInvoker(handler, disposeHandler)
{
    void HandlerResponse(HttpResponseMessage rsp)
    {
        if (reverseProxyConfig.AddServerHeader)
        {
            rsp.Headers.Server.TryParseAdd(HttpHeaderServer);
        }
    }

    /// <inheritdoc/>
    public override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var rsp = base.Send(request, cancellationToken);
        HandlerResponse(rsp);
        return rsp;
    }

    /// <inheritdoc/>
    public sealed override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var rsp = await base.SendAsync(request, cancellationToken);
        HandlerResponse(rsp);
        return rsp;
    }
}
