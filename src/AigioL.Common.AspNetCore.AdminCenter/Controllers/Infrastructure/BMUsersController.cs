using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Models.Users;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台的用户管理
/// </summary>
public static partial class BMUsersController
{
    const string ControllerName = "SystemUser";

    public static void MapBMUsers<TUser>(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/users") where TUser : BMUser, new()
    {
        var routeGroup = b.MapGroup(pattern)
        .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
        .WithDescription("管理后台的用户管理");

        routeGroup.MapGet("", async (HttpContext context, [FromQuery] int current = IPagedModel.DefaultCurrent, [FromQuery] int pageSize = IPagedModel.DefaultPageSize, [FromQuery] string? userName = null, [FromQuery] string? nickName = null, [FromQuery] string? name = null) =>
        {
            var r = await Get(context, current, pageSize, userName, nickName, name);
            return r.SetHttpContext(context);
        })
        .PermissionFilter(ControllerName, BMButtonType.Query)
        .WithDescription("查询管理后台的用户");
        routeGroup.MapPost("", async (HttpContext context, [FromBody] AddBMUserModel model) =>
        {
            var tenantId = TenantConstants.RootTenantIdG;
            var r = await Post<TUser>(context, model, tenantId);
            return r.SetHttpContext(context);
        })
        .WithDescription("新增管理后台的用户");
        routeGroup.MapPut("{id}", async (HttpContext context, [FromRoute] Guid id, [FromBody] EditBMUserModel model) =>
        {
            var r = await Put<TUser>(context, id, model);
            return r.SetHttpContext(context);
        })
        .WithDescription("修改管理后台的用户");
    }

    static async Task<BMApiRsp<PagedModel<BMUserTableItem>>> Get(HttpContext context, int current, int pageSize, string? userName = null, string? nickName = null, string? name = null)
    {
        var repo = context.RequestServices.GetRequiredService<IBMUserRepository>();
        var r = await repo.QueryAsync(userName, nickName, name, current, pageSize);
        return r;
    }

    static async Task<BMApiRsp> Post<TUser>(HttpContext context, AddBMUserModel model, Guid tenantId) where TUser : BMUser, new()
    {
        if (string.IsNullOrWhiteSpace(model.UserName))
        {
            return "请输入用户名";
        }
        if (string.IsNullOrWhiteSpace(model.Password1))
        {
            return "请输入密码";
        }

        var appSettings = context.RequestServices.GetRequiredService<IOptions<BMAppSettings>>().Value;

#if ENABLE_ALL_PWD_DECRYPT
        var rsaPrivateKey = appSettings.AdminRSAPrivateKey;
        ArgumentNullException.ThrowIfNull(rsaPrivateKey);
        var rsaParameters = RSAUtils.ReadParameters(rsaPrivateKey);
        using var rsa = RSA.Create(rsaParameters);

        var password1 = BMMinimalApis.DecryptAC(rsa, model.Password1);
        var password2 = BMMinimalApis.DecryptAC(rsa, model.Password2);
#else
        var password1 = model.Password1;
        var password2 = model.Password2;
#endif


        if (password1 != password2)
        {
            return "两次输入的密码不一致";
        }


        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();
        var user = await userManager.FindByNameAsync(model.UserName);
        if (user != null)
        {
            return $"用户名 {model.UserName} 已存在";
        }

        var repo = context.RequestServices.GetRequiredService<IBMUserRepository>();
        using var transaction = repo.Database.BeginTransaction();
        user = new()
        {
            UserName = model.UserName,
            TenantId = tenantId,
        };
        var createResult = await userManager.CreateAsync(user, password1);
        if (!createResult.Succeeded)
        {
            return createResult;
        }

        var hasRoles = model.Roles != null && model.Roles.Count != 0;
        if (hasRoles)
        {
            var identityResult = await userManager.AddToRolesAsync(user, model.Roles!);
            if (!identityResult.Succeeded)
            {
                return identityResult;
            }
        }

        await transaction.CommitAsync(CancellationToken.None);
        return HttpStatusCode.OK;
    }

    static async Task<BMApiRsp> Put<TUser>(HttpContext context, Guid id, EditBMUserModel model) where TUser : BMUser
    {
        var userId = context.GetACUserId();
        if (userId == id)
            return "不能编辑自己，请在个人中心修改用户名或密码";

        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return HttpStatusCode.NotFound;
        }

        var userName = await userManager.GetUserNameAsync(user);
        var repo = context.RequestServices.GetRequiredService<IBMUserRepository>();
        using var transaction = repo.Database.BeginTransaction();
        if (model.UserName != userName)
        {
            var identityResult = await userManager.SetUserNameAsync(user, model.UserName);
            if (!identityResult.Succeeded)
            {
                return identityResult;
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        var hasRoles = roles != null && roles.Any();
        var hasEditRoles = model.Roles != null && model.Roles.Count != 0;

        if (hasRoles && hasEditRoles && roles!.SequenceEqual(model.Roles!))
        {
            // 权限数据一致，不更改
        }
        else if (!hasRoles && !hasEditRoles)
        {
            // 无权限变更
        }
        else
        {
            if (hasRoles)
            {
                // 删除现有的权限
                var identityResult = await userManager.RemoveFromRolesAsync(user, roles!);
                if (!identityResult.Succeeded)
                {
                    return identityResult;
                }
            }

            if (hasEditRoles)
            {
                // 添加新增的权限
                var identityResult = await userManager.AddToRolesAsync(user, model.Roles!);
                if (!identityResult.Succeeded)
                {
                    return identityResult;
                }
            }
        }

        await transaction.CommitAsync(CancellationToken.None);
        return HttpStatusCode.OK;
    }
}
