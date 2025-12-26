using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles.AddOrEditArticleTagModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles.ArticleTagTableItemModel;

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
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("文章标签管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] string? name = null,
            [FromQuery] string? createUser = null,
            [FromQuery] string? operatorUser = null,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var createTime = context.GetQueryDateTimeRange("createTime");
            var updateTime = context.GetQueryDateTimeRange("updateTime");
            var articleTagRepo = context.RequestServices.GetRequiredService<IArticleTagRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await articleTagRepo.QueryAsync(
                name, createTime, updateTime,
                createUser, operatorUser, orderBy,
                desc, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询文章标签");

        routeGroup.MapGet("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var articleTagRepo = context.RequestServices.GetRequiredService<IArticleTagRepository>();
            BMApiRsp<AddOrEditM?> r = await articleTagRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("获取文章标签详情");

        routeGroup.MapPut("{id}", async (HttpContext context,
            [FromRoute] Guid id,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var articleTagRepo = context.RequestServices.GetRequiredService<IArticleTagRepository>();
            BMApiRsp r = await articleTagRepo.UpdateAsync(userId, id, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改文章标签");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var articleTagRepo = context.RequestServices.GetRequiredService<IArticleTagRepository>();
            BMApiRsp r = await articleTagRepo.InsertAsync(userId, model, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增文章标签");

        routeGroup.MapPost("addtags", async (HttpContext context,
            [FromBody] string[] tags) =>
        {
            var userId = context.GetBMUserId();
            var articleTagRepo = context.RequestServices.GetRequiredService<IArticleTagRepository>();
            BMApiRsp<Guid[]> r = await articleTagRepo.InsertAsync(userId, tags, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增多个文章标签");

        routeGroup.MapDelete("{id}", async (HttpContext context,
            [FromRoute] Guid id) =>
        {
            var articleTagRepo = context.RequestServices.GetRequiredService<IArticleTagRepository>();
            var rowCount = await articleTagRepo.DeleteAsync(id);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Delete)
        .WithDescription("删除文章标签");

        routeGroup.MapGet("options", async (HttpContext context) =>
        {
            var articleTagRepo = context.RequestServices.GetRequiredService<IArticleTagRepository>();
            BMApiRsp<ArticleTagOptionItemModel[]?> r = await articleTagRepo.QueryOptionsAsync(
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询文章标签的选项");
    }
}
