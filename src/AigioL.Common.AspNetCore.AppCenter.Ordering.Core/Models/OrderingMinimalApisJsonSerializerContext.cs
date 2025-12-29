using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

#region AftersalesBillController
[JsonSerializable(typeof(AftersalesBillAddModel))]
[JsonSerializable(typeof(ApiRsp<AftersalesBillDetailModel?>))]
#endregion
#region OrderingController
[JsonSerializable(typeof(ApiRsp<OrderPayInfoModel?>))]
#endregion
#region UserOrderController
[JsonSerializable(typeof(ApiRsp<OrderDetailModel?>))]
[JsonSerializable(typeof(ApiRsp<PagedModel<OrderItemInfoModel>?>))]
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
