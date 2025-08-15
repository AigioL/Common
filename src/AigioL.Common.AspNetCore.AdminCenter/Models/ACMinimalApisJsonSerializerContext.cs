using AigioL.Common.AspNetCore.AdminCenter.Models.Menus;
using AigioL.Common.AspNetCore.AdminCenter.Models.Users;
using AigioL.Common.JsonWebTokens.Models;
using AigioL.Common.Primitives.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

[JsonSerializable(typeof(ApiRspAC))]
[JsonSerializable(typeof(ApiRspAC<bool>))]
[JsonSerializable(typeof(ApiRspAC<byte>))]
[JsonSerializable(typeof(ApiRspAC<sbyte>))]
[JsonSerializable(typeof(ApiRspAC<ushort>))]
[JsonSerializable(typeof(ApiRspAC<short>))]
[JsonSerializable(typeof(ApiRspAC<uint>))]
[JsonSerializable(typeof(ApiRspAC<int>))]
[JsonSerializable(typeof(ApiRspAC<ulong>))]
[JsonSerializable(typeof(ApiRspAC<long>))]
[JsonSerializable(typeof(ApiRspAC<Guid>))]
[JsonSerializable(typeof(ApiRspAC<float>))]
[JsonSerializable(typeof(ApiRspAC<double>))]
[JsonSerializable(typeof(ApiRspAC<decimal>))]
[JsonSerializable(typeof(ApiRspAC<DateOnly>))]
[JsonSerializable(typeof(ApiRspAC<DateTime>))]
[JsonSerializable(typeof(ApiRspAC<DateTimeOffset>))]
[JsonSerializable(typeof(ApiRspAC<bool?>))]
[JsonSerializable(typeof(ApiRspAC<byte?>))]
[JsonSerializable(typeof(ApiRspAC<sbyte?>))]
[JsonSerializable(typeof(ApiRspAC<ushort?>))]
[JsonSerializable(typeof(ApiRspAC<short?>))]
[JsonSerializable(typeof(ApiRspAC<uint?>))]
[JsonSerializable(typeof(ApiRspAC<int?>))]
[JsonSerializable(typeof(ApiRspAC<ulong?>))]
[JsonSerializable(typeof(ApiRspAC<long?>))]
[JsonSerializable(typeof(ApiRspAC<Guid?>))]
[JsonSerializable(typeof(ApiRspAC<float?>))]
[JsonSerializable(typeof(ApiRspAC<double?>))]
[JsonSerializable(typeof(ApiRspAC<decimal?>))]
[JsonSerializable(typeof(ApiRspAC<DateOnly?>))]
[JsonSerializable(typeof(ApiRspAC<DateTime?>))]
[JsonSerializable(typeof(ApiRspAC<DateTimeOffset?>))]
[JsonSerializable(typeof(ApiRspAC<string>))]
[JsonSerializable(typeof(ApiRspAC<nil>))]
[JsonSerializable(typeof(ApiRspAC<nil?>))]
[JsonSerializable(typeof(InitSystemRequest))]
[JsonSerializable(typeof(ApiRspAC<JsonWebTokenValue>))]
#region BMMenusController
[JsonSerializable(typeof(ApiRspAC<List<ACMenuTreeItem>?>))]
[JsonSerializable(typeof(ApiRspAC<ACMenuModel?>))]
[JsonSerializable(typeof(ACMenuEdit))]
[JsonSerializable(typeof(ApiRspAC<List<ACMenuModel>?>))]
[JsonSerializable(typeof(ApiRspAC<List<ACButtonModel>?>))]
[JsonSerializable(typeof(ApiRspAC<List<Guid>?>))]
[JsonSerializable(typeof(IEnumerable<Guid>))]
[JsonSerializable(typeof(IEnumerable<ACButtonModel>))]
#endregion
#region BMRolesController
[JsonSerializable(typeof(ApiRspAC<List<SelectItemModel<Guid>>?>))]
[JsonSerializable(typeof(ApiRspAC<PagedModel<ACRoleModel>?>))]
[JsonSerializable(typeof(ApiRspAC<List<Guid>?>))]
[JsonSerializable(typeof(ACRoleModel))]
#endregion
#region BMUserController
[JsonSerializable(typeof(ApiRspAC<ACUserInfoModel>))]
[JsonSerializable(typeof(ApiRspAC<List<Guid>?>))]
[JsonSerializable(typeof(EditACUserInfoModel))]
#endregion
#region BMUsersController
[JsonSerializable(typeof(ApiRspAC<PagedModel<ACUserTableItem>>))]
[JsonSerializable(typeof(AddACUserModel))]
[JsonSerializable(typeof(EditACUserModel))]
#endregion
[JsonSourceGenerationOptions]
public sealed partial class ACMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static ACMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new ACMinimalApisJsonSerializerContext(o);
    }
}