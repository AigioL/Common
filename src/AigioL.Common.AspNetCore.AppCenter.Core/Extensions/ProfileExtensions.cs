using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AutoMapper;

/// <summary>
/// AutoMapper Configuration
/// <para>https://docs.automapper.io/en/stable/Configuration.html</para>
/// </summary>
public static partial class ProfileExtensions
{
    public static void AddCoreProfile(this Profile p)
    {
        p.CreateMap<Komaasharu, KomaasharuEdit>()
            .ReverseMap();

        p.CreateMap<Komaasharu, KomaasharuTableItem>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.Expired, opt => opt.MapFrom(s => DateTimeOffset.UtcNow >= s.EndTime));
    }
}
