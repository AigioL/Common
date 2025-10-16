using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

public static partial class ManageController
{
    public static void MapIdentityManageV1(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/manage")
    {
        var routeGroup = b.MapGroup(pattern)
            .RequireAuthorization(MSMinimalApis.MSApiControllerBaseAuthorize)
            .WithRequiredSecurityKey();

        routeGroup.MapPost("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("刷新用户信息 V1");
        routeGroup.MapGet("refreshuserinfo", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("刷新用户信息 V1");
        // changebindemail
        routeGroup.MapPost("changebindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("换绑手机（安全验证）V1");
        routeGroup.MapPut("changebindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("绑定新手机号 V1");
        routeGroup.MapDelete("deleteaccount", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("注销（删除）账号 V1");
        routeGroup.MapPost("clockin", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("每日签到 V1");
        routeGroup.MapGet("clockinrecords", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("获取每日签到记录 V1");
        routeGroup.MapPost("bindphonenumber", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("绑定手机号 V1");
        routeGroup.MapPost("setPassword", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("设置账号密码 V1");
        routeGroup.MapPost("edituserprofile", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("编辑个人资料 V1");
        routeGroup.MapGet("signout", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("退出登录（登出）V1");
        routeGroup.MapDelete("unbundleaccount/{channel}", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("解绑账号 V1");
        routeGroup.MapPost("sendbindemail", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("发送绑定邮箱邮件 V1");
        routeGroup.MapPost("bindemail", async (HttpContext context) =>
        {
            var r = await RefreshUserInfo(context);
            return r;
        }).WithDescription("绑定邮箱 V1");
    }
}
