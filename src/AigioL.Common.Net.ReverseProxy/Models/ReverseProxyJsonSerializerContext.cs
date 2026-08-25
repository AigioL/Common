using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Net.ReverseProxy.Models;

[JsonSerializable(typeof(FlowStatistics))]
public sealed partial class ReverseProxyJsonSerializerContext : JsonSerializerContext
{
    static ReverseProxyJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new ReverseProxyJsonSerializerContext(o);
    }
}
