using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;
using AigioL.Common.AspNetCore.AdminCenter.Models.Users;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

[JsonSerializable(typeof(BMApiRsp))]
[JsonSerializable(typeof(BMApiRsp<bool>))]
[JsonSerializable(typeof(BMApiRsp<byte>))]
[JsonSerializable(typeof(BMApiRsp<sbyte>))]
[JsonSerializable(typeof(BMApiRsp<ushort>))]
[JsonSerializable(typeof(BMApiRsp<short>))]
[JsonSerializable(typeof(BMApiRsp<uint>))]
[JsonSerializable(typeof(BMApiRsp<int>))]
[JsonSerializable(typeof(BMApiRsp<ulong>))]
[JsonSerializable(typeof(BMApiRsp<long>))]
[JsonSerializable(typeof(BMApiRsp<Guid>))]
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
[JsonSerializable(typeof(BMApiRsp<nil>))]
[JsonSerializable(typeof(BMApiRsp<nil?>))]
[JsonSerializable(typeof(BMInitSystemRequest))]
[JsonSerializable(typeof(BMApiRsp<JsonWebTokenValue>))]
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
#region KomaasharuController
[JsonSerializable(typeof(BMApiRsp<PagedModel<KomaasharuTableItem>?>))]
[JsonSerializable(typeof(KomaasharuEdit))]
[JsonSerializable(typeof(BMApiRsp<KomaasharuEdit?>))]
[JsonSerializable(typeof(BMApiRsp<StatisticsKomaasharuResponse[]?>))]
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