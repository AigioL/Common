using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using EditM = AigioL.Common.AspNetCore.AppCenter.Identity.Models.UserEdit;
using TableItemM = AigioL.Common.AspNetCore.AppCenter.Identity.Models.UserTableItem;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Identity;

/// <summary>
/// 客户端用户管理
/// </summary>
public static partial class UsersController
{
    const string ControllerName = ControllerConstants.ClientUser;

    public static void MapACUsers(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "ms/identity/users")
    {
        var routeGroup = b.MapGroup(pattern)
            .WithDescription("客户端用户管理");

        routeGroup.MapGet("", async (HttpContext context,
            [FromQuery] UserType? userType,
            [FromQuery] Guid? id,
            [FromQuery] string? openId,
            [FromQuery] string? phoneNumber,
            [FromQuery] string? nickName,
            [FromQuery] Gender? gender,
            [FromQuery] DateTimeOffset?[]? lastLoginTime,
            [FromQuery] bool? isLockout,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? desc = null,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var startTime = context.GetQueryDateTimeRangeNullable("startTime");
            var endTime = context.GetQueryDateTimeRangeNullable("endTime");
            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
            BMApiRsp<PagedModel<TableItemM>?> r = await userRepo.QueryAsync(
                id, openId, userType,
                nickName, gender, lastLoginTime,
                isLockout, phoneNumber, orderBy,
                desc, current, pageSize,
                cancellationToken: context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("分页查询客户端用户");

        routeGroup.MapGet("{userId}", async (HttpContext context,
            [FromRoute] Guid userId) =>
        {
            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
            BMApiRsp<EditM?> r = await userRepo.GetEditByIdAsync(userId, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("查询客户端用户详情");

        routeGroup.MapGet("{userId}/wallet", async (HttpContext context,
            [FromRoute] Guid userId) =>
        {
            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
            BMApiRsp<UserWalletModel?> r = await userRepo.GetWalletByUserIdAsync(userId, context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Detail)
        .WithDescription("查询客户端用户钱包详情");

        routeGroup.MapPut("", async (HttpContext context,
            [FromBody] EditM model) =>
        {
            var connection = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
            var authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();
            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
            // 高级权限：用户具备删除权限
            AuthorizationPolicy policy = new PermissionAuthorizationRequirement(ControllerName, BMButtonType.Delete);
            var superPermission = await authorizationService.AuthorizeAsync(context.User, null, policy);

            var rowCount = superPermission.Succeeded
                ? await userRepo.UpdateElevatedAsync(model)
                : await userRepo.UpdateAsync(model);

            // 清空用户信息缓存
            var redisDb = connection.GetDatabase(CacheKeys.RedisHashDataDb);
            await redisDb.HashDeleteAsync(CacheKeys.IdentityUserInfoDataHashV1Key,
                ShortGuid.Encode(model.Id));
            var r = rowCount > 0;
            return BMApiRsp.OkBoolean(r);
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("编辑客户端用户");

        routeGroup.MapPut("{userId}", async (HttpContext context,
            [FromRoute] Guid userId,
            [FromQuery] bool lockout) =>
        {
            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
            var isOk = await userRepo.SetUserLockoutStateAsync(userId, lockout);
            var r = BMApiRsp.OkBoolean(isOk);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Edit)
        .WithDescription("设置客户端用户封禁状态");

        routeGroup.MapGet("searchusers", async (HttpContext context,
            [FromQuery] string user) =>
        {
            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
            BMApiRsp<UserSearchModel?> r = await userRepo.SearchUsers(user,
               cancellationToken: context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("搜索客户端用户");

        routeGroup.MapGet("{userId}/walletchangerecords", async (HttpContext context,
            [FromRoute] Guid? userId,
            [FromQuery] UserWalletValueEvent[]? @event,
            [FromQuery] UserWalletValueType[]? @type,
            [FromQuery] UserWalletPaymentDirection? direction,
            [FromQuery] string? note,
            [FromQuery] string? sourceId,
            [FromQuery] bool? noticeStatus,
            [FromQuery] DateTimeOffset?[]? createTime,
            [FromQuery] int current = IPagedModel.DefaultCurrent,
            [FromQuery] int pageSize = IPagedModel.DefaultPageSize) =>
        {
            var userWalletChangeRecordRepo = context.RequestServices.GetRequiredService<IUserWalletChangeRecordRepository>();
            BMApiRsp<PagedModel<UserWalletChangeRecordModel>?> r = await userWalletChangeRecordRepo.QueryAsync(
                userId, @event, @type,
                direction, note, sourceId,
                noticeStatus, createTime, current,
                pageSize,
                context.RequestAborted);
            return r;
        }).PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询用户钱包变更记录");
    }
}
