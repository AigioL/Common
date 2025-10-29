using AigioL.Common.Models;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiRspProblemDetails(this IServiceCollection services, Action<ProblemDetailsOptions>? configure = null)
    {
        // https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.1.25451.107/src/Http/Http.Extensions/src/ProblemDetailsServiceCollectionExtensions.cs#L42
        services.Add(ServiceDescriptor.Singleton<IProblemDetailsWriter, ApiRspProblemDetailsWriter>());
        services.AddProblemDetails(configure);
        return services;
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
        await apiRsp.WriteAsync(
            httpContext.Response,
            cancellationToken: httpContext.RequestAborted);
    }
}