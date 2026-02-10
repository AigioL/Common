using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using TencentCloudSendSmsRequest = AigioL.Common.SmsSender.Models.Channels.TencentCloud.SendSmsRequest;

namespace AigioL.Common.SmsSender.Models.Channels.TencentCloud;

[JsonSerializable(typeof(TencentCloudSendSmsRequest))]
public sealed partial class TencentCloudSmsSenderJsonSerializerContext : JsonSerializerContext
{
    static TencentCloudSmsSenderJsonSerializerContext()
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

            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // 忽略 null 值
        };
        Default = new TencentCloudSmsSenderJsonSerializerContext(o);
    }
}
