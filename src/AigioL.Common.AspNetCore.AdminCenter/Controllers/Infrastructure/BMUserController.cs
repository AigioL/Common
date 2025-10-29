using AigioL.Common.AspNetCore.AdminCenter.Constants;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Models.Users;
using AigioL.Common.AspNetCore.AdminCenter.Repositories.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

/// <summary>
/// 管理后台当前登录的后台用户个人资料修改
/// </summary>
public static partial class BMUserController
{
    const string DefaultACUserAvatarUrl = "/img/default-avatar.png";

    public static void MapBMUser<TUser>(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "bm/user") where TUser : BMUser
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(BMMinimalApis.ApiControllerBaseAuthorize)
            .WithDescription("管理后台的登录用户个人资料管理");

        routeGroup.MapGet("", async (HttpContext context) =>
        {
            var tenantId = TenantConstants.RootTenantIdG;
            var r = await Get<TUser>(context, tenantId);
            return r.SetHttpContext(context);
        })
        .WithDescription("获取当前登录管理后台的用户个人资料");
        routeGroup.MapPut("", async (HttpContext context, [FromBody] EditBMUserInfoModel model) =>
        {
            var r = await Put<TUser>(context, model);
            return r.SetHttpContext(context);
        })
        .WithDescription("编辑当前登录管理后台的用户个人资料");
        routeGroup.MapGet("menus", async (HttpContext context) =>
        {
            var tenantId = TenantConstants.RootTenantIdG;
            var r = await GetRoleMenus<TUser>(context, tenantId);
            return r.SetHttpContext(context);
        })
        .WithDescription("获取当前登录管理后台的用户角色菜单主键列表");
        routeGroup.MapPut("cpwd", async (HttpContext context, [FromBody] BMChangePasswordRequest model) =>
        {
            var r = await Put<TUser>(context, model);
            return r.SetHttpContext(context);
        })
        .WithDescription("修改当前登录管理后台用户的密码（验证旧密码相同）");
    }

    static async Task<BMApiRsp<BMUserInfoModel>> Get<TUser>(HttpContext context, Guid tenantId) where TUser : BMUser
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return HttpStatusCode.Unauthorized;
        }
        ArgumentNullException.ThrowIfNull(user.UserName);

        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var menus = await repo.GetUserMenuAsync(user.Id, tenantId);

        BMUserInfoModel r = new()
        {
            UserName = user.UserName,
            NickName = user.NickName,
            Avatar = DefaultACUserAvatarUrl,
            TenantId = user.TenantId,
            Menus = menus,
        };
        return r;
    }

    static async Task<BMApiRsp> Put<TUser>(HttpContext context, EditBMUserInfoModel model) where TUser : BMUser
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return HttpStatusCode.Unauthorized;
        }
        ArgumentNullException.ThrowIfNull(user.UserName);

        if (user.UserName != model.UserName)
        {
            user.NickName = model.NickName;
            var identityResult = await userManager.SetUserNameAsync(user, model.UserName);
            if (!identityResult.Succeeded)
            {
                return identityResult;
            }
        }
        else if (user.NickName != model.NickName)
        {
            user.NickName = model.NickName;
            var identityResult = await userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                return identityResult;
            }
        }

        return HttpStatusCode.OK;
    }

    static async Task<BMApiRsp<List<Guid>?>> GetRoleMenus<TUser>(HttpContext context, Guid tenantId) where TUser : BMUser
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return HttpStatusCode.Unauthorized;
        }
        ArgumentNullException.ThrowIfNull(user.UserName);

        var repo = context.RequestServices.GetRequiredService<IBMMenuRepository>();
        var r = await repo.GetRoleMenus(user.Id, tenantId);
        return r;
    }

    static async Task<BMApiRsp> Put<TUser>(HttpContext context, BMChangePasswordRequest model) where TUser : BMUser
    {
        var appSettings = context.RequestServices.GetRequiredService<IOptions<BMAppSettings>>().Value;

#if ENABLE_ALL_PWD_DECRYPT
        var rsaPrivateKey = appSettings.AdminRSAPrivateKey;
        ArgumentNullException.ThrowIfNull(rsaPrivateKey);
        var rsaParameters = RSAUtils.ReadParameters(rsaPrivateKey);
        using var rsa = RSA.Create(rsaParameters);

        var oldPassword = BMMinimalApis.DecryptAC(rsa, model.OldPassword);
        var newPassword = BMMinimalApis.DecryptAC(rsa, model.NewPassword);
#else
        var oldPassword = model.OldPassword;
        var newPassword = model.NewPassword;
#endif


        var userManager = context.RequestServices.GetRequiredService<UserManager<TUser>>();
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return HttpStatusCode.Unauthorized;
        }
        ArgumentNullException.ThrowIfNull(user.UserName);

        var identityResult = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        if (!identityResult.Succeeded)
        {
            return identityResult;
        }

        return HttpStatusCode.OK;
    }
}
