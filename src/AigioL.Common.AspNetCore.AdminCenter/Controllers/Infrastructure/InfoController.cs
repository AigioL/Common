using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.JsonWebTokens.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AdminCenter.Controllers.Infrastructure;

public static partial class InfoController
{
    /// <summary>
    /// 创建一个默认系统管理员账号，且在 DEBUG 下将返回 JsonWebToken，用于测试
    /// </summary>
    /// <param name="b"></param>
    /// <param name="pattern"></param>
    public static void MapPostInfo(this IEndpointRouteBuilder b, [StringSyntax("Route")] string pattern = "api/info")
    {
        b.MapPost(pattern, (HttpContext context, [FromBody] BMInitSystemRequest model) =>
        {
            BMApiRsp<JsonWebTokenValue> result;
            try
            {
                // TODO: 实现初始化系统的逻辑
                result = null!;
            }
            catch (Exception ex)
            {
                result = new();
                result.SetException(ex);
            }
            return Results.Json(result, BMMinimalApisJsonSerializerContext.Default.BMApiRspJsonWebTokenValue);
        })
        .AllowAnonymous()
        .WithDescription("创建一个默认系统管理员账号");
    }
}
