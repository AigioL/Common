using AigioL.Common.AspNetCore.AdminCenter;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
#if NET10_0_OR_GREATER
#else
using Microsoft.OpenApi.Models;
#endif

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class OpenApiOptionsExtensions
{
    /// <summary>
    /// 添加 Bearer 安全方案（JWT）转换器到 OpenAPI 文档转换器
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static OpenApiOptions AddBMBearerSecuritySchemeTransformer(this OpenApiOptions options)
        => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
}

/// <summary>
/// https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/openapi/customize-openapi#use-document-transformers
/// </summary>
/// <param name="authenticationSchemeProvider"></param>
file sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == BMMinimalApis.BearerScheme))
        {
#if NET10_0_OR_GREATER
            var requirements = new Dictionary<string, IOpenApiSecurityScheme>
#else
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
#endif
            {
                [BMMinimalApis.BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = BMMinimalApis.BearerSchemeLower, // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }
    }
}