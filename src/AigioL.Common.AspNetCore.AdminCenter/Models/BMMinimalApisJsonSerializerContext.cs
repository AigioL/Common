using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;
using AigioL.Common.AspNetCore.AdminCenter.Models.Users;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

[JsonSerializable(typeof(BMApiRsp))]
[JsonSerializable(typeof(BMApiRsp<bool>))]
[JsonSerializable(typeof(BMApiRsp<bool[]>))]
[JsonSerializable(typeof(BMApiRsp<byte>))]
[JsonSerializable(typeof(BMApiRsp<sbyte>))]
[JsonSerializable(typeof(BMApiRsp<ushort>))]
[JsonSerializable(typeof(BMApiRsp<short>))]
[JsonSerializable(typeof(BMApiRsp<uint>))]
[JsonSerializable(typeof(BMApiRsp<int>))]
[JsonSerializable(typeof(BMApiRsp<int[]>))]
[JsonSerializable(typeof(BMApiRsp<ulong>))]
[JsonSerializable(typeof(BMApiRsp<long>))]
[JsonSerializable(typeof(BMApiRsp<Guid>))]
[JsonSerializable(typeof(BMApiRsp<Guid[]>))]
[JsonSerializable(typeof(BMApiRsp<float>))]
[JsonSerializable(typeof(BMApiRsp<double>))]
[JsonSerializable(typeof(BMApiRsp<decimal>))]
[JsonSerializable(typeof(BMApiRsp<DateOnly>))]
[JsonSerializable(typeof(BMApiRsp<DateTime>))]
[JsonSerializable(typeof(BMApiRsp<DateTimeOffset>))]
[JsonSerializable(typeof(BMApiRsp<bool?>))]
[JsonSerializable(typeof(BMApiRsp<byte?>))]
[JsonSerializable(typeof(BMApiRsp<sbyte?>))]
[JsonSerializable(typeof(BMApiRsp<ushort?>))]
[JsonSerializable(typeof(BMApiRsp<short?>))]
[JsonSerializable(typeof(BMApiRsp<uint?>))]
[JsonSerializable(typeof(BMApiRsp<int?>))]
[JsonSerializable(typeof(BMApiRsp<ulong?>))]
[JsonSerializable(typeof(BMApiRsp<long?>))]
[JsonSerializable(typeof(BMApiRsp<Guid?>))]
[JsonSerializable(typeof(BMApiRsp<float?>))]
[JsonSerializable(typeof(BMApiRsp<double?>))]
[JsonSerializable(typeof(BMApiRsp<decimal?>))]
[JsonSerializable(typeof(BMApiRsp<DateOnly?>))]
[JsonSerializable(typeof(BMApiRsp<DateTime?>))]
[JsonSerializable(typeof(BMApiRsp<DateTimeOffset?>))]
[JsonSerializable(typeof(BMApiRsp<string>))]
[JsonSerializable(typeof(BMApiRsp<string[]>))]
[JsonSerializable(typeof(BMApiRsp<nil>))]
[JsonSerializable(typeof(BMApiRsp<nil?>))]
[JsonSerializable(typeof(BMInitSystemRequest))]
[JsonSerializable(typeof(BMApiRsp<JsonWebTokenValue>))]
#region StatisticsController
[JsonSerializable(typeof(BMApiRsp<CachedStatistics<StatisticsPieResponse[]>?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsLineResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<ActiveUserSumResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsActiveUserOSResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsChartActiveUserOSResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsKomaasharuResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<UserActivityStatisticsResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsSmsUsageTrendResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsEmailUsageTrendResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsOrderAmountQtyModel[]?>))]
[JsonSerializable(typeof(BMApiRsp<OrderAmountQtyTableModel[]?>))]
[JsonSerializable(typeof(BMApiRsp<AnalysisResponse[]?>))]
[JsonSerializable(typeof(BMApiRsp<IdNameModel[]?>))]
#endregion
#region ArticleCategoryController
[JsonSerializable(typeof(BMApiRsp<PagedModel<ArticleCategoryTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<AddOrEditArticleCategoryModel>))]
[JsonSerializable(typeof(AddOrEditArticleCategoryModel))]
[JsonSerializable(typeof(BMApiRsp<ArticleCategoryTreeNodeModel[]?>))]
#endregion
#region ArticleController
[JsonSerializable(typeof(BMApiRsp<PagedModel<ArticleTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<AddOrEditArticleModel>))]
[JsonSerializable(typeof(AddOrEditArticleModel))]
[JsonSerializable(typeof(BMApiRsp<SelectItemModel<Guid>[]?>))]
[JsonSerializable(typeof(BMApiRsp<ArticleOptionItemModel[]?>))]
#endregion
#region ArticleTagController
[JsonSerializable(typeof(BMApiRsp<PagedModel<ArticleTagTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<AddOrEditArticleTagModel>))]
[JsonSerializable(typeof(AddOrEditArticleTagModel))]
[JsonSerializable(typeof(BMApiRsp<ArticleTagOptionItemModel[]?>))]
#endregion
#region AppVersionController
[JsonSerializable(typeof(BMApiRsp<PagedModel<AppVersionTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<AddOrEditAppVersionModel>))]
[JsonSerializable(typeof(AddOrEditAppVersionModel))]
#endregion
#region KeyValuePairController
[JsonSerializable(typeof(BMApiRsp<PagedModel<KeyValuePairTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<KeyValuePairTableItemModel>))]
[JsonSerializable(typeof(AddOrEditKeyValuePairModel))]
#endregion
#region OfficialMessageController
[JsonSerializable(typeof(BMApiRsp<PagedModel<OfficialMessageTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<AddOrEditOfficialMessageModel>))]
[JsonSerializable(typeof(AddOrEditOfficialMessageModel))]
#endregion
#region StaticResourceController
[JsonSerializable(typeof(BMApiRsp<PagedModel<StaticResourceTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<AddOrEditStaticResourceModel>))]
[JsonSerializable(typeof(AddOrEditStaticResourceModel))]
#endregion
#region AuthMessageRecordController
[JsonSerializable(typeof(BMApiRsp<PagedModel<AuthMessageRecordTableItem>?>))]
#endregion
#region ExternalAccountsController
[JsonSerializable(typeof(BMApiRsp<PagedModel<ExternalAccountTableItem>?>))]
#endregion
#region UserCancelsController
[JsonSerializable(typeof(BMApiRsp<PagedModel<UserDeleteTableItem>?>))]
#endregion
#region UserDevicesController
[JsonSerializable(typeof(BMApiRsp<PagedModel<UserDeviceTableItem>?>))]
#endregion
#region UsersController
[JsonSerializable(typeof(BMApiRsp<PagedModel<UserTableItem>?>))]
[JsonSerializable(typeof(UserEdit))]
[JsonSerializable(typeof(BMApiRsp<UserEdit?>))]
[JsonSerializable(typeof(BMApiRsp<UserWalletModel?>))]
[JsonSerializable(typeof(BMApiRsp<UserSearchModel?>))]
[JsonSerializable(typeof(BMApiRsp<PagedModel<UserWalletChangeRecordModel>?>))]
#endregion
#region BMMenusController
[JsonSerializable(typeof(BMApiRsp<List<BMMenuTreeItem>?>))]
[JsonSerializable(typeof(BMApiRsp<BMMenuModel?>))]
[JsonSerializable(typeof(BMMenuEdit))]
[JsonSerializable(typeof(BMApiRsp<List<BMMenuModel>?>))]
[JsonSerializable(typeof(BMApiRsp<List<BMButtonModel>?>))]
[JsonSerializable(typeof(BMApiRsp<List<Guid>?>))]
[JsonSerializable(typeof(IEnumerable<Guid>))]
[JsonSerializable(typeof(IEnumerable<BMButtonModel>))]
#endregion
#region BMRolesController
[JsonSerializable(typeof(BMApiRsp<List<SelectItemModel<Guid>>?>))]
[JsonSerializable(typeof(BMApiRsp<PagedModel<BMRoleModel>?>))]
[JsonSerializable(typeof(BMApiRsp<List<Guid>?>))]
[JsonSerializable(typeof(BMRoleModel))]
#endregion
#region BMUserController
[JsonSerializable(typeof(BMApiRsp<BMUserInfoModel>))]
[JsonSerializable(typeof(BMApiRsp<List<Guid>?>))]
[JsonSerializable(typeof(EditBMUserInfoModel))]
[JsonSerializable(typeof(BMChangePasswordRequest))]
#endregion
#region BMUsersController
[JsonSerializable(typeof(BMApiRsp<PagedModel<BMUserTableItem>>))]
[JsonSerializable(typeof(AddBMUserModel))]
[JsonSerializable(typeof(EditBMUserModel))]
#endregion
#region KomaasharuController
[JsonSerializable(typeof(BMApiRsp<PagedModel<KomaasharuTableItem>?>))]
[JsonSerializable(typeof(KomaasharuEdit))]
[JsonSerializable(typeof(BMApiRsp<KomaasharuEdit?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsKomaasharuResponse[]?>))]
#endregion
#region MembershipBusinessOrderController
[JsonSerializable(typeof(BMApiRsp<PagedModel<MembershipBusinessOrderTableItem>?>))]
#endregion
#region MembershipGoodsController
[JsonSerializable(typeof(BMApiRsp<PagedModel<MembershipGoodsTableItem>?>))]
[JsonSerializable(typeof(BMApiRsp<AddOrEditMembershipGoodsModel?>))]
[JsonSerializable(typeof(AddOrEditMembershipGoodsModel))]
#endregion
#region MembershipProductKeyRecordController
[JsonSerializable(typeof(BMApiRsp<PagedModel<MembershipProductKeyRecordTableItem>?>))]
#endregion
#region AftersalesBillController
[JsonSerializable(typeof(BMApiRsp<PagedModel<AftersalesBillTableItem>?>))]
#endregion
#region MerchantDeductionAgreementConfigurationController
[JsonSerializable(typeof(BMApiRsp<PagedModel<MerchantDeductionAgreementConfigurationTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<PagedModel<AddOrEditMerchantDeductionAgreementConfigurationModel>?>))]
[JsonSerializable(typeof(AddOrEditMerchantDeductionAgreementConfigurationModel))]
#endregion
#region MerchantDeductionAgreementController
[JsonSerializable(typeof(BMApiRsp<PagedModel<MerchantDeductionAgreementTableItemModel>?>))]
#endregion
#region OrderBusinessPaymentConfigurationController
[JsonSerializable(typeof(BMApiRsp<PagedModel<OrderBusinessPaymentConfigurationTableItemModel>?>))]
[JsonSerializable(typeof(BMApiRsp<PagedModel<AddOrEditOrderBusinessPaymentConfigurationModel>?>))]
[JsonSerializable(typeof(AddOrEditOrderBusinessPaymentConfigurationModel))]
#endregion
#region OrderController
[JsonSerializable(typeof(BMApiRsp<PagedModel<OrderTableItem>?>))]
#endregion
#region RefundBillController
[JsonSerializable(typeof(BMApiRsp<PagedModel<RefundBillTableItemModel>?>))]
[JsonSerializable(typeof(AddRefundBillModel))]
#endregion
[JsonSourceGenerationOptions]
public sealed partial class BMMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static BMMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new BMMinimalApisJsonSerializerContext(o);
    }
}