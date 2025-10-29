using AigioL.Common.AspNetCore.AdminCenter.Models;
using System.Collections.Immutable;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddBMApiRspProblemDetails(this IServiceCollection services, Action<ProblemDetailsOptions>? configure = null)
    {
        // https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.1.25451.107/src/Http/Http.Extensions/src/ProblemDetailsServiceCollectionExtensions.cs#L42
        services.Add(ServiceDescriptor.Singleton<IProblemDetailsWriter, BMApiRspProblemDetailsWriter>());
        services.AddProblemDetails(configure);
        return services;
    }
}

/// <summary>
/// https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.1.25451.107/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs
/// </summary>
file sealed class BMApiRspProblemDetailsWriter : IProblemDetailsWriter
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
        string[] errorMessages = [];
        IDictionary<string, string[]> modelState = ImmutableDictionary<string, string[]>.Empty;
        if (context.ProblemDetails is HttpValidationProblemDetails httpValidationProblemDetails)
        {
            // https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.1.25451.107/src/Http/Routing/src/ValidationEndpointFilterFactory.cs#L99
            status = StatusCodes.Status400BadRequest;
            errorMessages = [.. httpValidationProblemDetails.Errors.Values
                .SelectMany(static x => x)
                .Where(static x => !string.IsNullOrWhiteSpace(x))];
            modelState = httpValidationProblemDetails.Errors;
        }
        else if (context.ProblemDetails.Status.HasValue)
        {
            status = context.ProblemDetails.Status.Value;
        }
        else
        {
            status = httpContext.Response.StatusCode;
        }

        BMApiRsp apiRsp = new()
        {
            Code = unchecked((uint)status),
            Url = httpContext.Request.Path,
            TraceId = traceId,
            Messages = errorMessages,
            ModelState = modelState,
        };
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        await httpContext.Response.WriteAsJsonAsync(apiRsp,
            BMMinimalApisJsonSerializerContext.Default.BMApiRsp,
            cancellationToken: httpContext.RequestAborted);
    }
}