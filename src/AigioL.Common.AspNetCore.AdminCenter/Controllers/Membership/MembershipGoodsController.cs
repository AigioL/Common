using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using AddOrEditM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership.AddOrEditMembershipGoodsModel;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership.MembershipGoodsTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Membership;

/// <summary>
/// 会员商品管理
/// </summary>
public static partial class MembershipGoodsController
{
    const string ControllerName = ControllerConstants.MembershipGoods;

    public static void MapMembershipGoods(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/membership/goods")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("会员商品管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] Guid? id,
            [FromQuery] string? goodsName,
            [FromQuery] string? goodsNo,
            [FromQuery] MembershipLicenseFlags? memberLicenseType,
            [FromQuery] int? rechargeDays,
            [FromQuery] decimal? currentPrice,
            [FromQuery] bool? enable,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await membershipGoodsRepo.QueryAsync(
                id, goodsName, goodsNo,
                memberLicenseType, rechargeDays, currentPrice,
                enable, current, pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询会员商品");

        routeGroup.MapGet("{id}", async (HttpContext context,
          [FromRoute] Guid id) =>
        {
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            BMApiRsp<AddOrEditM?> r = await membershipGoodsRepo.GetEditByIdAsync(id, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
      .WithDescription("获取会员商品详情");

        routeGroup.MapPut("{id?}", async (HttpContext context,
            [FromRoute] Guid? id,
            [FromBody] AddOrEditM model) =>
        {
            if (id.HasValue)
            {
                model.Id = id.Value;
            }
            var userId = context.GetBMUserId();
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            BMApiRsp r = await membershipGoodsRepo.UpdateAsync(userId, model, context.RequestAborted);
            if (r.IsSuccess)
            {
                // 刷新会员商品缓存
                var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                await RefreshGoodsCacheAsync(connection);
            }
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("修改会员商品");

        routeGroup.MapPost("", async (HttpContext context,
            [FromBody] AddOrEditM model) =>
        {
            var userId = context.GetBMUserId();
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            BMApiRsp r = await membershipGoodsRepo.InsertAsync(userId, model, context.RequestAborted);
            if (r.IsSuccess)
            {
                // 刷新会员商品缓存
                var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                await RefreshGoodsCacheAsync(connection);
            }
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Add)
        .WithDescription("新增会员商品");

        //routeGroup.MapDelete("{id}", async (HttpContext context,
        //    [FromRoute] Guid id) =>
        //{
        //    var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
        //    var rowCount = await membershipGoodsRepo.DeleteAsync(id);
        //    BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
        //    if (r.IsSuccess)
        //    {
        //        // 刷新会员商品缓存
        //        var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        //        await RefreshGoodsCacheAsync(connection);
        //    }
        //    return r;
        //}).PermissionFilter(ControllerName, BMButtonType.Delete)
        //.WithDescription("删除会员商品");

        routeGroup.MapPut("setenable/{id}/{enable}", async (HttpContext context,
            [FromRoute] Guid id,
            [FromRoute] bool enable) =>
        {
            var userId = context.GetBMUserId();
            var membershipGoodsRepo = context.RequestServices.GetRequiredService<IMembershipGoodsRepository>();
            int rowCount = await membershipGoodsRepo.EnabledMembershipGoodsAsync(id, enable, userId);
            BMApiRsp<bool> r = BMApiRsp.OkBoolean(rowCount > 0);
            if (r.IsSuccess)
            {
                // 刷新会员商品缓存
                var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                await RefreshGoodsCacheAsync(connection);
            }
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("上架或下架会员商品");
    }

    static async Task RefreshGoodsCacheAsync(IConnectionMultiplexer connection)
    {
        var database = connection.GetDatabase(CacheKeys.RedisMessagingDb);
        var cacheKey = CacheKeys.GetMembershipGoodsCacheKey;
        await database.KeyDeleteAsync(cacheKey);
    }
}
