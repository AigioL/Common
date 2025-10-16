using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Response;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

#pragma warning disable CS0618 // 类型或成员已过时
#region AuthMessageController OR VerificationCodesController
[JsonSerializable(typeof(SendSmsRequestV0))]
[JsonSerializable(typeof(SendEmailCodeRequestV0))]
[JsonSerializable(typeof(SendSmsRequestV1))]
[JsonSerializable(typeof(SendEmailCodeRequestV1))]
#endregion
#region AccountController
[JsonSerializable(typeof(LoginOrRegisterRequestV_1))]
[JsonSerializable(typeof(LoginOrRegisterRequestV0))]
[JsonSerializable(typeof(LoginOrRegisterRequestV2))]
[JsonSerializable(typeof(ApiRsp<LoginOrRegisterResponseV0?>))]
[JsonSerializable(typeof(ApiRsp<LoginOrRegisterResponseV_1?>))]
[JsonSerializable(typeof(ApiRsp<JsonWebTokenValue?>))]
[JsonSerializable(typeof(RefreshTokenRequestV0))]
[JsonSerializable(typeof(RefreshTokenRequestV1))]
[JsonSerializable(typeof(ValidateRegisterEmailRequestV0))]
[JsonSerializable(typeof(ValidateRegisterEmailRequestV2))]
[JsonSerializable(typeof(ResetPasswordRequestV0))]
[JsonSerializable(typeof(ResetPasswordRequestV2))]
[JsonSerializable(typeof(RegisterByEmailRequestV0))]
[JsonSerializable(typeof(RegisterByEmailRequestV2))]
[JsonSerializable(typeof(AccountLoginRequestV0))]
[JsonSerializable(typeof(AccountLoginRequestV2))]
#endregion
#region ManageController
#endregion
#region MembershipController
[JsonSerializable(typeof(ApiRsp<MembershipInfoV1?>))]
[JsonSerializable(typeof(ApiRsp<MembershipInfoV2?>))]
#endregion
#pragma warning restore CS0618 // 类型或成员已过时
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
