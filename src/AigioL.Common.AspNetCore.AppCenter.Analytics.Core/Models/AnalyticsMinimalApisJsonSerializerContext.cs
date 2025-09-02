using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.ActiveUsers;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models;

#region LogsController
[JsonSerializable(typeof(AnalysisLogContainerModel))]
#endregion
#region ActiveUsersController
[JsonSerializable(typeof(ActiveUserRecordModel))]
#endregion
[JsonSourceGenerationOptions]
public sealed partial class AnalyticsMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static AnalyticsMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new AnalyticsMinimalApisJsonSerializerContext(o);
    }
}
