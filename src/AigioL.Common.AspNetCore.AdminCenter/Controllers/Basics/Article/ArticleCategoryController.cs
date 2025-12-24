using AigioL.Common.AspNetCore.AdminCenter.Constants;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Basics.Article;

/// <summary>
/// 文章分类管理
/// </summary>
public static partial class ArticleCategoryController
{
    const string ControllerName = ControllerConstants.ArticleCategory;

    public static void MapArticleCategory(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/basics/articlecategories")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("文章分类管理");
    }
}
