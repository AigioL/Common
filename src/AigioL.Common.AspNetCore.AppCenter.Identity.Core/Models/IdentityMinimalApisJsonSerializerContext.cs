using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

#region AuthMessageController OR VerificationCodesController
[JsonSerializable(typeof(SendSmsRequest))]
[JsonSerializable(typeof(SendEmailCodeRequest))]
[JsonSerializable(typeof(SendSmsRequestV1))]
[JsonSerializable(typeof(SendEmailCodeRequestV1))]
#endregion
#region AccountController
[JsonSerializable(typeof(LoginOrRegisterRequest))]
[JsonSerializable(typeof(ApiRsp<LoginOrRegisterResponse?>))]
#endregion
#region ManageController
#endregion
#region MembershipController
[JsonSerializable(typeof(ApiRsp<MembershipInfoV1?>))]
#endregion
[JsonSourceGenerationOptions]
public sealed partial class IdentityMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static IdentityMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new IdentityMinimalApisJsonSerializerContext(o);
    }
}
