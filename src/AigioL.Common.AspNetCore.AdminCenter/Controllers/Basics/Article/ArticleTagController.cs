using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics.Article;

/// <summary>
/// 文章标签管理
/// </summary>
public static partial class ArticleTagController
{
    const string ControllerName = ControllerConstants.ArticleCategory;

    public static void MapArticleTag(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/articletags")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("文章标签管理");
    }
}
