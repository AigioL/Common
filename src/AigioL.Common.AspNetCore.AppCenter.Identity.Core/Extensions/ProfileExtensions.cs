using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;

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
        p.CreateMap<AuthMessageRecord, AuthMessageRecordTableItem>()
            .ForMember(d => d.UserInfo, opt => opt.MapFrom(s => s.User));
        p.CreateMap<ExternalAccount, ExternalAccountTableItem>();
        p.CreateMap<UserDelete, UserDeleteTableItem>()
            .ForMember(d => d.UserInfo, opt => opt.MapFrom(s => s.User))
            .ForMember(d => d.ExternalAccounts, opt => opt.MapFrom(s => s.ExternalAccounts));
        p.CreateMap<UserDevice, UserDeviceTableItem>();
        p.CreateMap<User, UserTableItem>();
        p.CreateMap<User, UserEdit>();
        p.CreateMap<UserWallet, UserWalletModel>();
        p.CreateMap<UserWalletChangeRecord, UserWalletChangeRecordModel>();
    }
}

#if DEBUG
[Obsolete("use AddIdentityProfile", true)]
public class IdentityUserProfile;
#endif