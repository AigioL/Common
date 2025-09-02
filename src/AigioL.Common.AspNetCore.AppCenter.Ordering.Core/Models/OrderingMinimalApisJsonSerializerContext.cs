using AigioL.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

#region AftersalesBillController
[JsonSerializable(typeof(AftersalesBillAddDto))]
[JsonSerializable(typeof(ApiRsp<AftersalesBillDetailModel?>))]
#endregion
#region OrderingController
[JsonSerializable(typeof(ApiRsp<OrderPayInfoModel?>))]
#endregion
[JsonSourceGenerationOptions]
public sealed partial class OrderingMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static OrderingMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new OrderingMinimalApisJsonSerializerContext(o);
    }
}
