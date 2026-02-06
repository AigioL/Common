using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles.AddOrEditArticleCategoryModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles.ArticleCategoryTableItemModel;

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
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("文章分类管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? parentId = null,
            [FromQuery] string? name = null,
            [FromQuery] long? sort = null,
            [FromQuery] string? createUser = null,
            [FromQuery] string? operatorUser = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] DateTimeOffset[]? createTime = null,
            [FromQuery] DateTimeOffset[]? updateTime = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var articleCategoryRepo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await articleCategoryRepo.QueryAsync(
                parentId, name, sort,
                createTime, updateTime, createUser,
                operatorUser, orderBy, desc,
                current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询文章分类");

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var articleCategoryRepo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            BMApiRsp<AddOrEditM?> r = await articleCategoryRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("获取文章分类详情");

        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] AddOrEditM model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }
            var userId = context.GetBMUserId();
            var articleCategoryRepo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            BMApiRsp r = await articleCategoryRepo.UpdateAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改文章分类");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var articleCategoryRepo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            BMApiRsp r = await articleCategoryRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增文章分类");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var articleCategoryRepo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            var rowCount = await articleCategoryRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除文章分类");

        routeGroup.MapGet("tree", async (HttpContext context) =>
        {
            var articleCategoryRepo = context.RequestServices.GetRequiredService<IArticleCategoryRepository>();
            BMApiRsp<ArticleCategoryTreeNodeModel[]> r = await articleCategoryRepo.GetTreeAsync(context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("获取文章分类树节点");
    }
}
