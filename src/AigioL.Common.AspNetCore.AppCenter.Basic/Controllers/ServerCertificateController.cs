using AigioL.Common.AspNetCore.AppCenter.Basic.Models.ApiGateway;
using MemoryPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class ServerCertificateController
{
    public static void MapBasicServerCertificateValidate(
        this IEndpointRouteBuilder b)
    {
        b.MapGet("/.well-known/acme-challenge/{token}", (HttpContext context,
            [FromRoute] string token) =>
        {
            var r = Validate(context, token);
            return r;
        }).AllowAnonymous()
        .WithDescription("SSL 证书域名所有权验证")
            .Produces(StatusCodes.Status200OK, contentType: "text/plain; charset=utf-8")
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// SSL 证书域名所有权验证
    /// </summary>
    /// <param name="context"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    static IResult Validate(HttpContext context, string token)
    {
        var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(ServerCertificateController));
        LogInfoValidateToken(logger, token);
        try
        {
            var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
            if (!string.IsNullOrWhiteSpace(token) && TryGetValue(context, cache, token, out var value))
            {
                return Results.Content(value);
            }
        }
        catch (Exception ex)
        {
            LogInfoValidateCatch(logger, ex);
            return Results.InternalServerError();
        }
        return Results.Unauthorized();
    }

    static bool TryGetValue(
        HttpContext context,
        IDistributedCache cache,
        string token,
        [NotNullWhen(true)] out string? value)
    {
        var bytes = cache.Get(ServerCertificateValidateKey.CacheKey);
        if (bytes?.Length > 0)
        {
            var dict = MemoryPackSerializer.Deserialize<ImmutableDictionary<ServerCertificateValidateKey, string>>(bytes);
            if (dict != null)
            {
                string? requestHost = context.Request.Host.HasValue ? context.Request.Host.Host : null;
                ServerCertificateValidateKey key = new()
                {
                    MatchDomainName = requestHost,
                    Token = token,
                };
                if (dict.TryGetValue(key, out value))
                {
                    return true;
                }
            }
        }
        value = default;
        return false;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Validate token:{token}")]
    private static partial void LogInfoValidateToken(ILogger logger, string token);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Validate error")]
    private static partial void LogInfoValidateCatch(ILogger logger, Exception exception);
}
