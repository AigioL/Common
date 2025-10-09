using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.AspNetCore.AppCenter.Security;
using AigioL.Common.Models;
using MemoryPack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Authentication;
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

    static bool Equals(MediaType l, MediaType r) => l.Type == r.Type && l.SubType == r.SubType;

    public static bool TryParse(
        StringValues contentTypeOrAccept,
        out SerializableImplType serializableImplType) => TryParse(contentTypeOrAccept, out var _,
            out serializableImplType,
            out var _,
            out var _);

    public static bool TryParse(
        StringValues contentTypeOrAccept,
        out bool isSecurity,
        out SerializableImplType serializableImplType,
        out SecurityKeyAlgorithmType algorithmType,
        [NotNullWhen(true)] out string? responseContentType)
    {
        if (!StringValues.IsNullOrEmpty(contentTypeOrAccept))
        {
            foreach (var it in contentTypeOrAccept)
            {
                if (string.IsNullOrWhiteSpace(it))
                {
                    continue;
                }
                var parsedContentType = new MediaType(it);
                if (Equals(parsedContentType, new(MediaTypeNames.MemoryPack)))
                {
                    isSecurity = false;
                    serializableImplType = SerializableImplType.MemoryPack;
                    responseContentType = MediaTypeNames.MemoryPack;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.MemoryPackSecurity)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.MemoryPack;
                    algorithmType = SecurityKeyAlgorithmType.RSAWithRandomAes;
                    responseContentType = MediaTypeNames.MemoryPackSecurity;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.MemoryPackSecurityECDiffieHellman)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.MemoryPack;
                    algorithmType = SecurityKeyAlgorithmType.ECDHSharedKeyWithRandomIV;
                    responseContentType = MediaTypeNames.MemoryPackSecurityECDiffieHellman;
                }
                //else if (Equals(parsedContentType, new(MediaTypeNames.MessagePack)))
                //{
                //    isSecurity = false;
                //    serializableImplType = SerializableImplType.MessagePack;
                //}
                //else if (Equals(parsedContentType, new(MediaTypeNames.MessagePackSecurity)))
                //{
                //    isSecurity = true;
                //    serializableImplType = SerializableImplType.MessagePack;
                //    algorithmType = SecurityKeyAlgorithmType.RsaKeyX;
                //}
                //else if (Equals(parsedContentType, new(MediaTypeNames.MessagePackSecurityECDiffieHellman)))
                //{
                //    isSecurity = true;
                //    serializableImplType = SerializableImplType.MessagePack;
                //    algorithmType = SecurityKeyAlgorithmType.DiffieHellman;
                //}
                else if (Equals(parsedContentType, new(MediaTypeNames.JSON)))
                {
                    isSecurity = false;
                    serializableImplType = SerializableImplType.SystemTextJson;
                    responseContentType = MediaTypeNames.JSON;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.JSONSecurity)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.SystemTextJson;
                    algorithmType = SecurityKeyAlgorithmType.RSAWithRandomAes;
                    responseContentType = MediaTypeNames.JSONSecurity;
                }
                else if (Equals(parsedContentType, new(MediaTypeNames.JSONSecurityECDiffieHellman)))
                {
                    isSecurity = true;
                    serializableImplType = SerializableImplType.SystemTextJson;
                    algorithmType = SecurityKeyAlgorithmType.ECDHSharedKeyWithRandomIV;
                    responseContentType = MediaTypeNames.JSONSecurityECDiffieHellman;
                }
            }
        }
        isSecurity = false;
        serializableImplType = default;
        algorithmType = default;
        responseContentType = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteApiRspAsync(
        SerializableImplType serializableImplType,
        HttpResponse response,
        ApiRsp value,
        CancellationToken cancellationToken = default)
        => WriteApiRspAsync(
            serializableImplType,
            response,
            value,
            MSMinimalApisJsonSerializerContext.Default.ApiRsp,
            cancellationToken);

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
        SerializableImplType serializableImplType,
        HttpResponse response,
        TValue value,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
        where TValue : ApiRsp
    {
        switch (serializableImplType)
        {
            case SerializableImplType.MemoryPack:
                {
                    // https://github.com/Cysharp/MemoryPack/blob/1.21.4/src/MemoryPack.AspNetCoreMvcFormatter/MemoryPackOutputFormatter.cs#L55
                    var writer = response.BodyWriter;
                    response.Headers.ContentType = MediaTypeNames.MemoryPack;
                    MemoryPackSerializer.Serialize(typeof(TValue), writer, value);
                    await writer.FlushAsync(cancellationToken);
                }
                break;
            case SerializableImplType.SystemTextJson:
            default:
                {
                    await response.WriteAsJsonAsync(value,
                        jsonTypeInfo,
                        cancellationToken: cancellationToken);
                }
                break;
        }
    }

    public static Task WriteApiRspAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>(
        HttpResponse response,
        TValue value,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
        where TValue : ApiRsp
    {
        if (!TryParse(response.HttpContext.Request.Headers.Accept, out var serializableImplType))
        {
            serializableImplType = SerializableImplType.SystemTextJson;
        }
        return WriteApiRspAsync(serializableImplType, response, value, jsonTypeInfo, cancellationToken);
    }

    /// <summary>
    /// 使用 <see cref="ApiRsp"/> 格式的异常处理与状态码响应
    /// <para>https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/error-handling</para>
    /// </summary>
    /// <param name="app"></param>
    /// <param name="handlerException">可重写异常处理，返回 <see langword="true"/> 中断 <see cref="ApiRsp"/> 格式的行为</param>
    /// <returns></returns>
    public static IApplicationBuilder UseApiRspExceptionHandler(
        this IApplicationBuilder app,
        Func<HttpContext, Task<bool>>? handlerException = null)
        => app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                if (handlerException != null)
                {
                    var isReturn = await handlerException(context);
                    if (isReturn)
                    {
                        return;
                    }
                }

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

    public static IServiceCollection AddApiRspProblemDetails(this IServiceCollection services, Action<ProblemDetailsOptions>? configure = null)
    {
        // https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.1.25451.107/src/Http/Http.Extensions/src/ProblemDetailsServiceCollectionExtensions.cs#L42
        services.Add(ServiceDescriptor.Singleton<IProblemDetailsWriter, ApiRspProblemDetailsWriter>());
        services.AddProblemDetails(configure);
        return services;
    }
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
#if NET10_0_OR_GREATER
            var requirements = new Dictionary<string, IOpenApiSecurityScheme>
#else
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
#endif
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
/// https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.1.25451.107/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs
/// </summary>
file sealed class ApiRspProblemDetailsWriter : IProblemDetailsWriter
{
    public bool CanWrite(ProblemDetailsContext context)
    {
        return true;
    }

    public async ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var httpContext = context.HttpContext;
        var traceId = httpContext.GetTraceId();

        int status;
        string? errorMessage = null;
        if (context.ProblemDetails is HttpValidationProblemDetails httpValidationProblemDetails)
        {
            // https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.1.25451.107/src/Http/Routing/src/ValidationEndpointFilterFactory.cs#L99
            status = StatusCodes.Status400BadRequest;
            errorMessage = httpValidationProblemDetails.Errors.Values
                .SelectMany(static x => x)
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .FirstOrDefault(); // 取第一个错误消息
        }
        else if (context.ProblemDetails.Status.HasValue)
        {
            status = context.ProblemDetails.Status.Value;
        }
        else
        {
            status = httpContext.Response.StatusCode;
        }

        ApiRsp apiRsp = new()
        {
            Code = unchecked((uint)status),
            Url = httpContext.Request.Path,
            TraceId = traceId,
            Message = errorMessage,
        };
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        await MSMinimalApis.WriteApiRspAsync(
            httpContext.Response,
            apiRsp,
            cancellationToken: httpContext.RequestAborted);
    }
}