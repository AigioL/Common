using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AutoMapper;

/// <summary>
/// AutoMapper Configuration
/// <para>https://docs.automapper.io/en/stable/Configuration.html</para>
/// </summary>
public static partial class ProfileExtensions
{
    public static void AddOrderingProfile(this Profile p)
    {
        p.CreateMap<MembershipBusinessOrder, MembershipBusinessOrderTableItem>();

        p.CreateMap<MembershipGoods, MembershipGoodsTableItem>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.Configurations, opt => opt.MapFrom(s => s.MerchantDeductionAgreementConfigurations != null ? s.MerchantDeductionAgreementConfigurations.Select(s => s.Id).ToList() : new()));

        p.CreateMap<MembershipProductKeyRecord, MembershipProductKeyRecordTableItem>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName));
    }
}
