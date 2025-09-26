using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models;

#region ArticleController
[JsonSerializable(typeof(ApiRsp<ArticleCategoryTreeModel[]?>))]
[JsonSerializable(typeof(ApiRsp<PagedModel<ArticleItemModel>?>))]
[JsonSerializable(typeof(ApiRsp<ArticleModel?>))]
#endregion
#region CustomerServiceController
#endregion
#region OfficialMessageController
[JsonSerializable(typeof(ApiRsp<PagedModel<OfficialMessageItemModel>?>))]
#endregion
#region ServerCertificateController
#endregion
#region ImageController
#endregion
#region VersionsController
#endregion
[JsonSourceGenerationOptions]
public sealed partial class BasicMinimalApisJsonSerializerContext : JsonSerializerContext
{
    static BasicMinimalApisJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new BasicMinimalApisJsonSerializerContext(o);
    }
}
