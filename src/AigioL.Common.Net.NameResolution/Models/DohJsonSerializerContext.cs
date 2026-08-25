using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Net.NameResolution.Models;

[JsonSerializable(typeof(DohJsonResponseModel))]
internal sealed partial class DohJsonSerializerContext : JsonSerializerContext
{
    static DohJsonSerializerContext()
    {
        JsonSerializerOptions o = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };
        // 这里所有的模型类都需要用 JsonPropertyName 标注
        Default = new DohJsonSerializerContext(o);
    }
}
