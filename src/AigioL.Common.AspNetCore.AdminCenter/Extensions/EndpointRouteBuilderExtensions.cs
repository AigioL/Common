using AigioL.Common.AspNetCore.AdminCenter.Controllers.Analysis;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics.Article;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Komaasharu;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Membership;
using AigioL.Common.AspNetCore.AdminCenter.Controllers.Ordering;
using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 注册管理后台的最小 API 路由
    /// </summary>
    public static void MapBMMinimalApis<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDbContext,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUser,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRole,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUserRole>(this IEndpointRouteBuilder b,
        bool ignoreInfoController = false,
        bool enablePwdLogin = true,
        bool enableSmsLogin = true)
        where TDbContext : BMDbContextBase<TUser, TRole, TUserRole>, IBMDbContextBase
        where TUser : BMUser, new()
        where TRole : BMRole, new()
        where TUserRole : BMUserRole
    {
        // Analysis
        b.MapAnalysisStatistics();

        // Basics/Article
        b.MapArticleCategory();
        b.MapArticle();
        b.MapArticleTag();

        // Basics
        b.MapAppVersion();
        b.MapClashProxyController();
        b.MapExchangeRate();
        b.MapKeyValuePair();
        b.MapOfficialMessage();
        b.MapStaticResource();

        // Identity
        b.MapAuthMessageRecord();
        b.MapACUserExternalAccounts();
        b.MapACUserCancels();
        b.MapACUserClockInRecords();
        b.MapACUserDevices();
        b.MapACUserExpRecords();
        b.MapACUsers();

        // Infrastructure
        if (!ignoreInfoController)
        {
            b.MapPostInfo<TDbContext, TUser, TRole, TUserRole>();
        }
        b.MapBMLogin<TUser>(enablePwdLogin: enablePwdLogin, enableSmsLogin: enableSmsLogin);
        b.MapBMMenus();
        b.MapBMRoles<TRole>();
        b.MapBMUser<TUser>();
        b.MapBMUsers<TUser>();

        // Komaasharu
        b.MapKomaasharu();
        b.MapKomaasharuPersonalizeds();

        // Membership
        b.MapMembershipBusinessOrder();
        b.MapMembershipGoods();
        b.MapMembershipProductKeyRecord();

        // Ordering
        b.MapAftersalesBill();
        b.MapCooperatorAccount();
        b.MapMerchantDeductionAgreementConfiguration();
        b.MapMerchantDeductionAgreement();
        b.MapOrderBusinessPaymentConfiguration();
        b.MapOrder();
        b.MapRefundBill();
    }
}
