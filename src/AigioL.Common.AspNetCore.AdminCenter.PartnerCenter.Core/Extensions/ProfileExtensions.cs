using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AutoMapper;

/// <summary>
/// AutoMapper Configuration
/// <para>https://docs.automapper.io/en/stable/Configuration.html</para>
/// </summary>
public static partial class ProfileExtensions
{
    public static void AddPartnerCenterProfile(this Profile p)
    {
        p.CreateMap<PCUser, PCUserTableItem>()
           .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
           .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName));
        p.CreateMap<PCUser, AddOrEditPCUserModel>()
            .ReverseMap();
    }
}
