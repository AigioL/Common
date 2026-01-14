using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Extensions.Http.Proxy.Models;

[JsonSerializable(typeof(WebProxyModel))]
public sealed partial class ExHttpProxyJsonSerializerContext : JsonSerializerContext
{
    static ExHttpProxyJsonSerializerContext()
    {
        JsonSerializerOptions o = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,

            #region JsonSerializerDefaults.Web https://github.com/dotnet/runtime/blob/v9.0.7/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L172-L174

            PropertyNameCaseInsensitive = true, // 忽略大小写
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // 允许从字符串读取数字

            #endregion
        };
        Default = new ExHttpProxyJsonSerializerContext(o);
    }
}
