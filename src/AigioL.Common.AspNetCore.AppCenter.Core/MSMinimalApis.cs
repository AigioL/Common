using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Models;
using MemoryPack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace AigioL.Common.AspNetCore.AppCenter;

public static partial class MSMinimalApis
{
    public const string BearerScheme = "Bearer";
    public const string BearerSchemeLower = "bearer";

    /// <summary>
    /// 添加 Bearer 安全方案（JWT）转换器到 OpenAPI 文档转换器
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static OpenApiOptions AddMSBearerSecuritySchemeTransformer(this OpenApiOptions options) => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();

    public static readonly AuthorizeAttribute MSApiControllerBaseAuthorize = new() { AuthenticationSchemes = BearerScheme, };

    public static SerializableImplType GetSerializableImplType(StringValues accept, out bool isSecurity)
    {
        isSecurity = false;
        if (!StringValues.IsNullOrEmpty(accept))
        {
            foreach (var it in accept)
            {
                var span = it.AsSpan();
                if (span.Equals(MediaTypeNames.MemoryPack, StringComparison.InvariantCultureIgnoreCase))
                {
                    return SerializableImplType.MemoryPack;
                }
                else if (span.Equals(MediaTypeNames.MemoryPackSecurity, StringComparison.InvariantCultureIgnoreCase))
                {
                    isSecurity = true;
                    return SerializableImplType.MemoryPack;
                }
                else if (span.Equals(MediaTypeNames.JSON, StringComparison.InvariantCultureIgnoreCase))
                {
                    return SerializableImplType.SystemTextJson;
                }
                else if (span.Equals(MediaTypeNames.JSONSecurity, StringComparison.InvariantCultureIgnoreCase) || span.Equals("application/vnd.sapi+text", StringComparison.InvariantCultureIgnoreCase))
                {
                    isSecurity = true;
                    return SerializableImplType.SystemTextJson;
                }
            }
        }
        return SerializableImplType.SystemTextJson;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteApiRspAsync(
        HttpResponse response,
        ApiRsp value,
        CancellationToken cancellationToken = default)
        => WriteApiRspAsync(
            response,
            value,
            MSMinimalApisJsonSerializerContext.Default.ApiRsp,
            cancellationToken);

    public static async Task WriteApiRspAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>(
        HttpResponse response,
        TValue value,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
        where TValue : ApiRsp
    {
        var t = GetSerializableImplType(response.HttpContext.Request.Headers.Accept, out var isSecurity);
        switch (t)
        {
            case SerializableImplType.MemoryPack:
                {
                    if (isSecurity)
                    {
                        throw new NotImplementedException("TODO: MemoryPack+Security");
                    }
                    else
                    {
                        // https://github.com/Cysharp/MemoryPack/blob/1.21.4/src/MemoryPack.AspNetCoreMvcFormatter/MemoryPackOutputFormatter.cs#L55
                        var writer = response.BodyWriter;
                        MemoryPackSerializer.Serialize(typeof(TValue), writer, value);
                        await writer.FlushAsync(cancellationToken);
                    }
                }
                break;
            case SerializableImplType.SystemTextJson:
            default:
                {
                    if (isSecurity)
                    {
                        throw new NotImplementedException("TODO: SystemTextJson+Security");
                    }
                    else
                    {
                        await response.WriteAsJsonAsync(value,
                            jsonTypeInfo,
                            cancellationToken: cancellationToken);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 使用 <see cref="ApiRsp"/> 格式的异常处理与状态码响应
    /// <para>https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/error-handling</para>
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseApiRspExceptionHandler(this IApplicationBuilder app) => app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var path = context.GetExceptionHandlerPath(out var error);
            ApiRsp apiRsp = new();
            if (error != null)
            {
                apiRsp.SetException(error);
            }
            else
            {
                apiRsp.Code = StatusCodes.Status500InternalServerError;
            }
            apiRsp.Url = path;
            var traceId = context.GetTraceId();
            apiRsp.TraceId = traceId;

            context.Response.StatusCode = StatusCodes.Status200OK;
            await WriteApiRspAsync(
                context.Response,
                apiRsp,
                cancellationToken: context.RequestAborted);
        });
    }).UseStatusCodePages(statusCodePagesApp =>
    {
        statusCodePagesApp.Run(async context =>
        {
            var traceId = context.GetTraceId();
            ApiRsp apiRsp = new()
            {
                Code = unchecked((uint)context.Response.StatusCode),
                Url = context.Request.Path,
                TraceId = traceId,
            };

            context.Response.StatusCode = StatusCodes.Status200OK;
            await WriteApiRspAsync(
                context.Response,
                apiRsp,
                cancellationToken: context.RequestAborted);
        });
    });
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
        if (authenticationSchemes.Any(authScheme => authScheme.Name == MSMinimalApis.BearerScheme))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                [MSMinimalApis.BearerScheme] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = MSMinimalApis.BearerSchemeLower, // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }
    }
}

/// <summary>
/// MIME 类型
/// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Basics_of_HTTP/MIME_Types</para>
/// </summary>
file static partial class MediaTypeNames
{
    /// <summary>
    /// application/json
    /// </summary>
    public const string JSON = "application/json";

    /// <summary>
    /// application/vnd.sapi+x-json
    /// </summary>
    public const string JSONSecurity = "application/vnd.sapi+x-json";

    /// <summary>
    /// application/x-memorypack
    /// </summary>
    public const string MemoryPack = "application/x-memorypack";

    /// <summary>
    /// application/vnd.sapi+x-memorypack
    /// </summary>
    public const string MemoryPackSecurity = "application/vnd.sapi+x-memorypack";
}