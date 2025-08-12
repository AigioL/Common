using AigioL.Common.SmsSender.Models.Channels._21VianetBlueCloud;
using AigioL.Common.SmsSender.Models.Channels.AlibabaCloud;
using AigioL.Common.SmsSender.Models.Channels.HuaweiCloud;
using AigioL.Common.SmsSender.Models.Channels.NetEaseCloud;
using AigioL.Common.SmsSender.Models.Channels.TencentCloud;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace AigioL.Common.SmsSender.Models;

[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(JsonValue))]
[JsonSerializable(typeof(JsonArray))]
[JsonSerializable(typeof(SendSmsResult))]
[JsonSerializable(typeof(SendSmsResult<SendSmsNetEaseCloudResult>))]
[JsonSerializable(typeof(CheckSmsResult))]
[JsonSerializable(typeof(CheckSmsResult<NetEaseCloudResult>))]
[JsonSerializable(typeof(_21VianetBlueRequestData))]
[JsonSerializable(typeof(SendSms21VianetBlueCloudResult))]
[JsonSerializable(typeof(SendSmsAlibabaCloudResult))]
[JsonSerializable(typeof(SendSmsNetEaseCloudResult))]
[JsonSerializable(typeof(NetEaseCloudResult))]
[JsonSerializable(typeof(SendHuaweiCloudResult))]
[JsonSerializable(typeof(SendSmsTencentCloudResult))]
[JsonSerializable(typeof(TencentCloudResult<SendSmsTencentCloudResult>))]
public sealed partial class SmsSenderJsonSerializerContext : JsonSerializerContext
{
    static SmsSenderJsonSerializerContext()
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
        Default = new SmsSenderJsonSerializerContext(o);
    }
}