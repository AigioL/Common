using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using System.Linq.Expressions;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AutoMapper;

/// <summary>
/// AutoMapper Configuration
/// <para>https://docs.automapper.io/en/stable/Configuration.html</para>
/// </summary>
public static partial class ProfileExtensions
{
    public static void AddIdentityProfile(this Profile p)
    {
        p.CreateMap<User, UserInfoModel>()
            .ForMember(d => d.MembershipInfo, opt => opt.MapFrom(s => s.Membership))
            .ForMember(d => d.Balance, opt => opt.MapFrom(s => s.Wallet.AccountBalance))
            .ForMember(d => d.HasPassword, opt => opt.MapFrom(s => s.PasswordHash != null))
            .ForMember(d => d.ExternalAccounts, opt => opt.MapFrom(s => s.ExternalAccounts));

        p.CreateMap<UserMembership, MembershipInfo>();

        p.CreateMap<AuthMessageRecord, AuthMessageRecordTableItem>()
            .ForMember(d => d.UserInfo, opt => opt.MapFrom(s => s.User));

        p.CreateMap<ExternalAccount, ExternalAccountTableItem>();
        p.CreateMap<ExternalAccount, ExternalAccountModel>();
        p.CreateMap<ExternalAccount, UserInfoExternalAccountModel>();

        p.CreateMap<UserDelete, UserDeleteTableItem>()
            .ForMember(d => d.UserInfo, opt => opt.MapFrom(s => s.User))
            .ForMember(d => d.ExternalAccounts, opt => opt.MapFrom(s => s.ExternalAccounts));

        p.CreateMap<UserDevice, UserDeviceTableItem>();

        p.CreateMap<User, UserTableItem>();

        p.CreateMap<User, UserEdit>();

        p.CreateMap<UserMembership, UserMembershipModel>();

        p.CreateMap<UserWallet, UserWalletModel>();

        p.CreateMap<UserWalletChangeRecord, UserWalletChangeRecordModel>();
    }

    //internal static readonly Expression<Func<User, UserInfoModel>> MapToUserInfoModel = user => new()
    //{
    //    Id = user.Id,
    //    NickName = user.NickName,
    //    //Experience = user.Experience,
    //    Balance = user.Wallet.AccountBalance,
    //    //Level = level,
    //    PersonalizedSignature = user.PersonalizedSignature,
    //    PhoneNumber = user.PhoneNumber,
    //    PhoneNumberRegionCode = user.PhoneNumberRegionCode,
    //    Gender = user.Gender,
    //    BirthDate = user.BirthDate.HasValue ? user.BirthDate.Value.DateTime : null,
    //    //BirthDateTimeZone = user.BirthDateTimeZone,
    //    // 当前登录用户不返回计算年龄值，由客户端本地计算
    //    AreaId = user.AreaId,
    //    AvatarUrl = user.AvatarUrl,
    //    UserType = user.UserType,
    //    //IsSignIn = lastSignInTime != default && lastSignInTime.Date.AddDays(1) > DateTimeOffset.UtcNow,
    //    //NextExperience = nextExperience,
    //    Email = user.Email,
    //    EmailConfirmed = user.EmailConfirmed,
    //    HasPassword = user.PasswordHash != null,

    //    MembershipInfo = membershipInfo,
    //    SteamAccountId = long.TryParse(user.ExternalAccounts!.FirstOrDefault(a => a.Type == ExternalLoginChannel.Steam)?.ExternalAccountId, out var steamId) ? steamId : null,
    //    MicrosoftAccountEmail = user.ExternalAccounts!.FirstOrDefault(a => a.Type == ExternalLoginChannel.Microsoft)?.Email ?? string.Empty,
    //    QQNickName = user.ExternalAccounts!.FirstOrDefault(a => a.Type == ExternalLoginChannel.QQ)?.NickName,
    //};
}

#if DEBUG
[Obsolete("use AddIdentityProfile", true)]
public class IdentityUserProfile;
#endif