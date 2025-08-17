using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace AigioL.Common.AspNetCore.AppCenter;

public static partial class MSMinimalApis
{
    /// <summary>
    /// 添加 Bearer 安全方案（JWT）转换器到 OpenAPI 文档转换器
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static OpenApiOptions AddMSBearerSecuritySchemeTransformer(this OpenApiOptions options) => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
}

/// <summary>
/// https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/openapi/customize-openapi#use-document-transformers
/// </summary>
/// <param name="authenticationSchemeProvider"></param>
file sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    const string BearerScheme = "Bearer";

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == BearerScheme))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                [BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = BearerScheme.ToLowerInvariant(), // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }
    }
}