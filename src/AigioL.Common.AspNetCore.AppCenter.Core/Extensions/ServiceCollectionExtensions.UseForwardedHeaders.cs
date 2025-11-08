using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using Microsoft.AspNetCore.HttpOverrides;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <inheritdoc cref="ForwardedHeadersExtensions.UseForwardedHeaders(IApplicationBuilder, ForwardedHeadersOptions)"/>
    public static IApplicationBuilder UseForwardedHeaders(this IApplicationBuilder builder, INotUseForwardedHeaders appSettings)
    {
        if (!appSettings.NotUseForwardedHeaders)
        {
            ForwardedHeadersOptions o = new()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false,
                ForwardLimit = null,
            };
            var knownProxies = appSettings.GetForwardedHeadersKnownProxies();
            foreach (var item in knownProxies)
            {
                o.KnownProxies.Add(item);
            }
            builder.UseForwardedHeaders(o);
        }
        return builder;
    }
}
