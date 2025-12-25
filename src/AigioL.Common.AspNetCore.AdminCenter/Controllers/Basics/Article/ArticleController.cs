using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles.AddOrEditArticleModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles.ArticleTableItemModel;

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

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? id = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] Guid? tagId = null,
            [FromQuery] string? title = null,
            [FromQuery] string? authorName = null,
            [FromQuery] string? introduction = null,
            [FromQuery] string? content = null,
            [FromQuery] string? createUser = null,
            [FromQuery] string? operatorUser = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRangeNullable("createTime");
            var updateTime = context.GetQueryDateTimeRangeNullable("updateTime");
            var articleRepo = context.RequestServices.GetRequiredService<IArticleRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await articleRepo.QueryAsync(
                id, categoryId, tagId,
                title, authorName, introduction,
                content, createTime, updateTime,
                createUser, operatorUser, orderBy,
                desc, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询文章");

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var articleRepo = context.RequestServices.GetRequiredService<IArticleRepository>();
            BMApiRsp<AddOrEditM?> r = await articleRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("获取文章详情");

        routeGroup.MapPut("{id}", async (HttpContext context,
            [FromRoute] Guid id,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var articleRepo = context.RequestServices.GetRequiredService<IArticleRepository>();
            BMApiRsp r = await articleRepo.UpdateAsync(userId, id, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改文章");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var articleRepo = context.RequestServices.GetRequiredService<IArticleRepository>();
            BMApiRsp r = await articleRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增文章");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var articleRepo = context.RequestServices.GetRequiredService<IArticleRepository>();
            var rowCount = await articleRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除文章");

        routeGroup.MapGet("select", async (HttpContext context) =>
        {
            var articleRepo = context.RequestServices.GetRequiredService<IArticleRepository>();
            BMApiRsp<SelectItemModel<Guid>[]?> r = await articleRepo.GetSelectAsync(
              cancellationToken: context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询文章的选择框");

        routeGroup.MapGet("options", async (HttpContext context,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] Guid? tagId = null,
            [FromQuery] string? title = null) =>
        {
            var articleRepo = context.RequestServices.GetRequiredService<IArticleRepository>();
            BMApiRsp<ArticleOptionItemModel[]?> r = await articleRepo.QueryOptionsAsync(
                categoryId, tagId, title,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询文章的选项");
    }
}
