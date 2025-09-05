using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

#region AgreementController
[JsonSerializable(typeof(ApiRsp<List<AgreementModel>?>))]
#endregion
#region MembershipController
[JsonSerializable(typeof(ApiRsp<MembershipGoodsModel[]?>))]
[JsonSerializable(typeof(MembershipOrderRequest))]
[JsonSerializable(typeof(MembershipCDKeyRequest))]
[JsonSerializable(typeof(MembershipCreateAgreementSignDeductRequest))]
#endregion
#region PaymentController
[JsonSerializable(typeof(OrderBusinessPaymentMethod))]
[JsonSerializable(typeof(ApiRsp<OrderBusinessPaymentMethod[]?>))]
#endregion
#region VipPaymentController
[JsonSerializable(typeof(ApiRsp<OrderStatus?>))]
#endregion
[JsonSourceGenerationOptions]
public sealed partial class PaymentMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static PaymentMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new PaymentMinimalApisJsonSerializerContext(o);
    }
}