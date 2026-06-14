using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Services.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.SmsSender.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using R = AigioL.Common.AspNetCore.AppCenter.Identity.UI.Properties.Resources;
using RModelValid = AigioL.Common.AspNetCore.AppCenter.Properties.ModelValidationErrors;

namespace AigioL.Common.AspNetCore.AppCenter.Helpers.Sms;

public static partial class SmsHelper
{
    /// <summary>
    /// 发送短信验证码
    /// 需要传入接收人手机号码以及用途(类型) 
    /// 根据不同的用途过滤手机号码是否符号发送短信条件
    /// 过滤不符合条件的不予发送短信
    /// </summary>
    public static async Task<ApiRsp> SendSms<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TAppSettings,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserManager2>(
        HttpContext context,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        SmsCodeType type,
        Action<AuthMessageRecord, Guid>? setUserId = null)
        where TAppSettings : class, IDisableSms
        where TUser : IdentityUser<Guid>
        where TUserManager2 : IIdentityUserManager<TUser>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return ApiRspCode.NetworkAuthenticationRequired;
        }
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(SmsHelper));
        var options = context.RequestServices.GetRequiredService<IOptions<TAppSettings>>();
        var isAuthenticated = context.User.Identity?.IsAuthenticated;
        var userManager = context.RequestServices.GetRequiredService<TUserManager2>();
        var authMessageRecordRepo = context.RequestServices.GetRequiredService<IAuthMessageRecordRepository>();
        var smsSender = context.RequestServices.GetRequiredService<ISmsSender>();
        var r = await SendSmsCoreAsync<TUser, TUserManager2>(
            logger, ipAddress, isAuthenticated,
            userManager, authMessageRecordRepo, smsSender,
            options.Value.DisableSms, phoneNumber, phoneNumberRegionCode,
            type, setUserId);
        return r;
    }

    static async Task<ApiRsp> SendSmsCoreAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserManager2>(
        ILogger logger,
        string ipAddress,
        bool? isAuthenticated,
        TUserManager2 userManager,
        IAuthMessageRecordRepository authMessageRecordRepo,
        ISmsSender smsSender,
        bool disableSms,
        string? phoneNumber,
        string? phoneNumberRegionCode,
        SmsCodeType type,
        Action<AuthMessageRecord, Guid>? setUserId = null,
        CancellationToken cancellationToken = default)
        where TUser : IdentityUser<Guid>
        where TUserManager2 : IIdentityUserManager<TUser>
    {
        if (disableSms)
        {
            return (ApiRspCode.SMSServerError, type switch
            {
                SmsCodeType.Login or SmsCodeType.Register or SmsCodeType.LoginOrRegister
                    => R.短信服务已停用_登录或注册,
                _ => R.短信服务已停用,
            });
        }

        if (string.IsNullOrWhiteSpace(phoneNumberRegionCode))
        {
            phoneNumberRegionCode = IPhoneNumber.DefaultPhoneNumberRegionCode;
        }

        // 手机号不能为 null
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return ApiRspCode.RequestModelValidateFail;
        }

        // 不支持的类型
        if (!Enum.IsDefined(type) || type == SmsCodeType.Register /*|| type == SmsCodeType.ForgotPassword*/)
        {
            return ApiRspCode.BadRequest;
        }

        var verify_phoneNumber = type switch // 验证传入的手机号是否必须有值
        {
            SmsCodeType.ChangePhoneNumberValidation => false,
            _ => true,
        };

        if (verify_phoneNumber)
        {
            if (phoneNumber == IPhoneNumber.SimulatorDefaultValue)
                return RModelValid.请输入正确的手机号码哦;
        }

        // 换绑手机号时，手机号填写 PhoneNumberHelper.SimulatorDefaultValue 固定值
        var verify_simulator_only = type switch // 是否验证手机号码必须为模拟器中的默认值
        {
            SmsCodeType.ChangePhoneNumberValidation => true,
            _ => false,
        };

        if (verify_simulator_only)
        {
            if (phoneNumber != IPhoneNumber.SimulatorDefaultValue)
            {
                return ApiRspCode.BadRequest;
            }
        }

        TUser? user = null; // 通过 JWT 授权 中的 Token 找到的用户
        var mustHasUser = type switch // 必须是通过 JWT 授权 中的 Token 找到了用户
        {
            SmsCodeType.ChangePhoneNumberValidation or
            SmsCodeType.ChangePhoneNumberNew or
            SmsCodeType.BindPhoneNumber => true,
            _ => false,
        };

        var useUserPhoneNumberReplaceRequest = type switch // 使用用户的手机号码替换请求中的值
        {
            SmsCodeType.ChangePhoneNumberValidation => true,
            _ => false,
        };

        if (mustHasUser)
        {
            if (!(isAuthenticated ?? false))
            {
                return ApiRspCode.Unauthorized;
            }
            user = await userManager.GetUserAsync();
            if (user == null) return ApiRspCode.Unauthorized;
            var isLock = await userManager.IsLockedOutAsync(user);
            if (isLock)
            {
                return ApiRspCode.UserIsBanOrLock;
            }
            if (useUserPhoneNumberReplaceRequest)
            {
                if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    return R.当前手机号码不存在;
                }
                phoneNumber = user.PhoneNumber;
            }
        }

        TUser? findUser = null;
        var isFindUser = false;

        async Task FindByPhoneNumberAsync()
        {
            if (!isFindUser && findUser == null)
            {
                findUser = await userManager.FindByPhoneNumberAsync(phoneNumber, phoneNumberRegionCode);
                isFindUser = true;
            }
        }

        if (type == SmsCodeType.LoginOrRegister)
        {
            await FindByPhoneNumberAsync();
            type = findUser == null ? SmsCodeType.Register : SmsCodeType.Login;
        }
        else if (type == SmsCodeType.Login)
        {
            // 登录时查询用户不存在不予发送短信验证码
            await FindByPhoneNumberAsync();
            if (findUser == null)
            {
                LogWranSMSOnLoginNotFoundUser(logger, phoneNumber, phoneNumberRegionCode);
                return true;
            }
        }

        var call_IsDuplicatePhoneNumber = type switch // 是否检查手机号码是否已存在
        {
            SmsCodeType.Register or
            SmsCodeType.ChangePhoneNumberNew or // 注册或更换手机号要保证手机号码不重复
            //SmsCodeType.ForgotPassword or
            SmsCodeType.BindPhoneNumber => true,
            _ => false,
        };
        var isMustDuplicatePhoneNumber = type switch // 手机号码已存在的情况下是否允许
        {
            //SmsCodeType.ForgotPassword => true,
            _ => false,
        };

        if (call_IsDuplicatePhoneNumber)
        {
            await FindByPhoneNumberAsync();
            var findUserIdByPhoneNum = findUser?.Id;
            var isDuplicatePhoneNumber = findUserIdByPhoneNum.HasValue && findUserIdByPhoneNum.Value != default;
            if (isDuplicatePhoneNumber)
            {
                if (!isMustDuplicatePhoneNumber) // 不允许传入的手机号码存在
                {
                    return type switch
                    {
                        SmsCodeType.Register => R.手机号码已存在_注册,
                        _ => R.手机号码已存在,
                    };
                }
                else
                {
                    if (isMustDuplicatePhoneNumber) // 找不到用户
                    {
                        return R.用户不存在;
                    }
                }
            }
        }

        //if (request.Type == SmsCodeType.Register) // 注册
        //{
        //    var errorInviteCode = InviteCode.GetErrorMessage(request.InviteCode);
        //    if (errorInviteCode != default)
        //    {
        //        return FailVerify(errorInviteCode);
        //    }
        //    var setInviteCodeResult = await inviteCodeRepository.SetUsePhoneNumber(request.InviteCode, request.PhoneNumber);
        //    if (!setInviteCodeResult)
        //    {
        //        return FailVerify(StringConstSharedant.邀请码已经被使用了哦);
        //    }
        //}

        if (type == SmsCodeType.BindPhoneNumber && (user == null || user.PhoneNumber != null))
        {
            return BindPhoneNumberIsNotNullError;
        }

        var lastSendTime = await authMessageRecordRepo.GetLastSendSmsTime(phoneNumber, phoneNumberRegionCode, type); // 上次发送短信时间
        if (lastSendTime.HasValue)
        {
            //if (DateTimeOffset.Now < lastSendTime.Value.AddSeconds(61))
            // 目前【1分钟内短信发送条数不超过：1】仅企业实名验证后才可改动
            var adds = lastSendTime.Value.AddSeconds(SMSConstants.SmsSendTooFrequently);
            var now = DateTimeOffset.Now;
            if (adds >= now)
            {
#if !DEBUG
                return R.当前手机号发送短信过于频繁_.Format(Math.Ceiling(TimeSpan.FromSeconds(SMSConstants.SmsSendTooFrequently).TotalMinutes));
#else
                logger.LogCritical("当前手机号发送短信过于频繁_");
#endif
            }
            else
            {
                var isMaxSendSmsDay = await authMessageRecordRepo.IsMaxSendSmsDay(phoneNumber, phoneNumberRegionCode);
                if (isMaxSendSmsDay)
                {
#if !DEBUG
                    return R.当前手机号今日发送短信数量超过最大上限;
#else
                    logger.LogCritical("当前手机号今日发送短信数量超过最大上限");
#endif
                }
            }
        }

        // ↓ 上一条有效纪录
        var lastEffectiveRecord = await authMessageRecordRepo
            .GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(
            AuthMessageType.PhoneNumber,
            phoneNumber,
            phoneNumberRegionCode,
            type);

        string message;
        if (lastEffectiveRecord != default) // 如果上一条有效纪录存在，那么将取上一次的随机数字再次发送
        {
            message = lastEffectiveRecord.Content;
        }
        else
        {
            var num = smsSender.GenerateRandomNum(MaxLengths.SMS_CAPTCHA);
            message = num.ToString();
        }

        var record = new AuthMessageRecord
        {
            PhoneNumber = phoneNumber,
            PhoneNumberRegionCode = phoneNumberRegionCode,
            Content = message,
            IPAddress = ipAddress,
            RequestType = type,
            Type = AuthMessageType.PhoneNumber,
        };

        if (findUser != null)
        {
            SetUserId(record, findUser.Id, setUserId);
        }
        else if (user != null)
        {
            SetUserId(record, user.Id, setUserId);
        }

        var smsResult = await smsSender.SendSmsAsync(
            phoneNumber, message, (ushort)type,
            cancellationToken);
        record.SendIsSuccess = smsResult.IsSuccess;
        record.SendResultRecord = smsResult.Result?.GetRecord();
        record.HttpStatusCode = smsResult.HttpStatusCode;
        record.Channel = smsSender.Channel;

        await authMessageRecordRepo.InsertAsync(record, CancellationToken.None);

        if (!record.SendIsSuccess)
        {
#pragma warning disable CA1873 // 避免进行可能成本高昂的日志记录
            LogErrorSMSServerError(logger,
                IPhoneNumber.ToStringHideMiddleFour(phoneNumber),
                phoneNumberRegionCode,
                record.Id,
                record.SendResultRecord);
#pragma warning restore CA1873 // 避免进行可能成本高昂的日志记录
            return (ApiRspCode.SMSServerError, R.短信服务故障);
        }
        else
        {
            return true;
        }
    }
}

partial class SmsHelper
{
    const string BindPhoneNumberIsNotNullError = "The mobile phone number of the current login user already exists.";

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "短信服务故障(5001)，手机号码：+{regionCode}{phoneNum}，ID：{id}，结果：{result}")]
    private static partial void LogErrorSMSServerError(ILogger logger, string? phoneNum, string? regionCode, Guid id, string? result);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "短信服务发送登录用途时找不到用户，手机号码：+{regionCode}{phoneNum}")]
    private static partial void LogWranSMSOnLoginNotFoundUser(ILogger logger, string? phoneNum, string? regionCode);

    static void SetUserId(AuthMessageRecord record, Guid userId, Action<AuthMessageRecord, Guid>? setUserId = null)
    {
        if (setUserId == null)
        {
            record.UserId = userId;
        }
        else
        {
            setUserId(record, userId);
        }
    }
}
