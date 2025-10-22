using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Yarp.ReverseProxy.Configuration;

namespace AigioL.Common.AspNetCore.ApiGateway.Models;

#region Yarp
[JsonSerializable(typeof(YarpReverseProxyConfigModel))]
[JsonSerializable(typeof(RouteConfig[]))]
[JsonSerializable(typeof(IReadOnlyList<RouteConfig>))]
[JsonSerializable(typeof(List<RouteConfig>))]
[JsonSerializable(typeof(ClusterConfig[]))]
[JsonSerializable(typeof(List<ClusterConfig>))]
[JsonSerializable(typeof(IReadOnlyList<ClusterConfig>))]
#endregion
[JsonSourceGenerationOptions]
public sealed partial class ApiGatewayMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static ApiGatewayMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new ApiGatewayMinimalApisJsonSerializerContext(o);
    }
}
