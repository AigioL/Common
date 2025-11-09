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
[JsonSerializable(typeof(SendSmsRequest))]
[JsonSerializable(typeof(SendEmailCodeRequest))]
#endregion
#region AccountController
[JsonSerializable(typeof(LoginOrRegisterRequest))]
[JsonSerializable(typeof(ApiRsp<LoginOrRegisterResponse?>))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(ApiRsp<JsonWebTokenValue?>))]
[JsonSerializable(typeof(ValidateRegisterEmailRequest))]
[JsonSerializable(typeof(ResetPasswordRequest))]
[JsonSerializable(typeof(RegisterByEmailRequest))]
[JsonSerializable(typeof(AccountLoginRequest))]
#endregion
#region ManageController
[JsonSerializable(typeof(ApiRsp<UserInfoModel?>))]
[JsonSerializable(typeof(ChangePhoneNumberValidationRequest))]
[JsonSerializable(typeof(ChangePhoneNumberNewRequest))]
[JsonSerializable(typeof(BindPhoneNumberRequest))]
[JsonSerializable(typeof(SetPasswordRequest))]
[JsonSerializable(typeof(EditUserProfileRequest))]
#endregion
#region MembershipController
[JsonSerializable(typeof(ApiRsp<MembershipInfo?>))]
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
