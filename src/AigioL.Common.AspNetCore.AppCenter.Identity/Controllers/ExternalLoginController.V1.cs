using AigioL.Common.AspNetCore.AppCenter.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Controllers;

static partial class ExternalLoginController
{
    public static void MapIdentityExternalLoginV1(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "identity/v1/externallogin")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("start/{channel?}", async (HttpContext context,
            [FromRoute] ExternalLoginChannel channel = ExternalLoginChannel.Steam) =>
        {
            var r = await ExternalLogin(context, channel);
            return r;
        }).WithIdentityUIView();
        routeGroup.MapGet("{channel?}", async (HttpContext context,
            [FromRoute] ExternalLoginChannel channel = ExternalLoginChannel.Steam) =>
        {
            var r = await ExternalLoginDetectionAsync(context, channel);
            return r;
        }).WithIdentityUIView();
        routeGroup.MapGet("callback", async (HttpContext context) =>
        {
            var r = await Callback(context);
            return r;
        }).WithIdentityUIView();

#if DEBUG
        routeGroup.MapGet("test/ex", async (HttpContext context) =>
        {
            await Task.Delay(10, context.RequestAborted);
            throw new ApplicationException("测试 WithIdentityUIView 的异常页面");
        }).WithIdentityUIView();
#endif
    }
}