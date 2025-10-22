using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Yarp.ReverseProxy.Configuration;

namespace AigioL.Common.AspNetCore.ApiGateway.Models;

/// <inheritdoc cref="IProxyConfig"/>
public sealed partial record class YarpReverseProxyConfigModel
{
    /// <inheritdoc cref="IProxyConfig.RevisionId"/>
    [JsonPropertyName("revisionId")]
    public required string RevisionId { get; init; }

    /// <inheritdoc cref="IProxyConfig.Routes"/>
    [JsonPropertyName("routes")]
    public required RouteConfig[] Routes { get; init; }

    /// <inheritdoc cref="IProxyConfig.Clusters"/>
    [JsonPropertyName("clusters")]
    public required ClusterConfig[] Clusters { get; init; }

    /// <inheritdoc/>
    public sealed override string ToString()
    {
        var result = JsonSerializer.Serialize(this,
            ApiGatewayMinimalApisJsonSerializerContext.Default.YarpReverseProxyConfigModel);
        return result;
    }

    /// <summary>
    /// 从 Json 字符串中解析 <see cref="YarpReverseProxyConfigModel"/>
    /// </summary>
    public static bool TryParse(string? s,
        [NotNullWhen(true)] out YarpReverseProxyConfigModel? result)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(s))
            {
                result = JsonSerializer.Deserialize(s,
                    ApiGatewayMinimalApisJsonSerializerContext.Default.YarpReverseProxyConfigModel);
                return result != null;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        result = null;
        return false;
    }

    /// <summary>
    /// 从字段的 Json 字符串中解析 <see cref="YarpReverseProxyConfigModel"/>
    /// </summary>
    public static bool TryParse(Guid revisionId, string? routes, string? clusters,
        [NotNullWhen(true)] out YarpReverseProxyConfigModel? result)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(routes) && !string.IsNullOrWhiteSpace(clusters))
            {
                var routes2 = JsonSerializer.Deserialize(routes,
                    ApiGatewayMinimalApisJsonSerializerContext.Default.RouteConfigArray);
                if (routes2 != null)
                {
                    var clusters2 = JsonSerializer.Deserialize(clusters,
                        ApiGatewayMinimalApisJsonSerializerContext.Default.ClusterConfigArray);
                    if (clusters2 != null)
                    {
                        result = new YarpReverseProxyConfigModel
                        {
                            RevisionId = revisionId.ToString(),
                            Routes = routes2,
                            Clusters = clusters2,
                        };
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        result = null;
        return false;
    }
}

partial record class YarpReverseProxyConfigModel : IProxyConfig
{
    /// <inheritdoc/>
    [JsonIgnore]
    IChangeToken IProxyConfig.ChangeToken => NullChangeToken.Singleton;

    /// <inheritdoc/>
    [JsonIgnore]
    IReadOnlyList<RouteConfig> IProxyConfig.Routes => Routes;

    /// <inheritdoc/>
    [JsonIgnore]
    IReadOnlyList<ClusterConfig> IProxyConfig.Clusters => Clusters;
}