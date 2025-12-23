using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics.Article;

/// <summary>
/// 文章管理
/// </summary>
public static partial class ArticleController
{
    const string ControllerName = ControllerConstants.Article;

    public static void MapArticle(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/articles")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("文章管理");
    }
}
