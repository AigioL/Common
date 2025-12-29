using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
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

        p.CreateMap<AddOrEditMembershipGoodsModel, MembershipGoods>();
        p.CreateMap<MembershipGoods, AddOrEditMembershipGoodsModel>()
            .ForMember(d => d.Configurations, opt => opt.MapFrom(s => s.MerchantDeductionAgreementConfigurations != null ? s.MerchantDeductionAgreementConfigurations.Select(s => s.Id).ToList() : new()));

        p.CreateMap<AftersalesBill, AftersalesBillTableItem>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.UserNickName, opt => opt.MapFrom(s => s.User == null ? default : s.User.NickName))
            .ForMember(d => d.UserPhoneNumber, opt => opt.MapFrom(s => s.User == null ? default : s.User.PhoneNumber))
            .ForMember(d => d.UserPhoneNumberRegionCode, opt => opt.MapFrom(s => s.User == null ? default : s.User.PhoneNumberRegionCode))
            .ForMember(d => d.UserEmail, opt => opt.MapFrom(s => s.User == null ? default : s.User.Email));

        p.CreateMap<MerchantDeductionAgreementConfiguration, MerchantDeductionAgreementConfigurationTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName));

        p.CreateMap<MerchantDeductionAgreementConfiguration, AddOrEditMerchantDeductionAgreementConfigurationModel>()
            .ReverseMap();

        p.CreateMap<MerchantDeductionAgreement, MerchantDeductionAgreementTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName));

        p.CreateMap<Order, OrderTableItem>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.UserNickName, opt => opt.MapFrom(s => s.User == null ? default : s.User.NickName))
            .ForMember(d => d.UserPhoneNumber, opt => opt.MapFrom(s => s.User == null ? default : s.User.PhoneNumber))
            .ForMember(d => d.UserPhoneNumberRegionCode, opt => opt.MapFrom(s => s.User == null ? default : s.User.PhoneNumberRegionCode))
            .ForMember(d => d.UserEmail, opt => opt.MapFrom(s => s.User == null ? default : s.User.Email));

        p.CreateMap<RefundBill, RefundBillTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.UserNickName, opt => opt.MapFrom(s => s.User == null ? default : s.User.NickName))
            .ForMember(d => d.UserPhoneNumber, opt => opt.MapFrom(s => s.User == null ? default : s.User.PhoneNumber))
            .ForMember(d => d.UserPhoneNumberRegionCode, opt => opt.MapFrom(s => s.User == null ? default : s.User.PhoneNumberRegionCode))
            .ForMember(d => d.AftersalesNumber, opt => opt.MapFrom(s => s.AftersalesBill == null ? default : s.AftersalesBill.AftersalesNumber))
            .ForMember(d => d.UserEmail, opt => opt.MapFrom(s => s.User == null ? default : s.User.Email))
            .ForMember(d => d.OrderNumber, opt => opt.MapFrom(s => (s.AftersalesBill == null && s.AftersalesBill!.Order == null) ? default : s.AftersalesBill.Order.Id))
            .ForMember(d => d.BusinessTypeId, opt => opt.MapFrom(s => (s.AftersalesBill == null && s.AftersalesBill!.Order == null) ? default : s.AftersalesBill.Order.BusinessTypeId))
            .ForMember(d => d.AmountReceived, opt => opt.MapFrom(s => (s.AftersalesBill == null && s.AftersalesBill!.Order == null) ? default : s.AftersalesBill.Order.AmountReceived));
    }
}
