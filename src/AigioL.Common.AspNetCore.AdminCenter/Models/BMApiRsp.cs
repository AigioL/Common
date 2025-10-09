using AigioL.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Immutable;
using System.Net;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 多租户管理后台的 WebApi 接口响应模型
/// </summary>
public partial record class BMApiRsp
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => (Code >= 200u) && (Code <= 299u);

    /// <summary>
    /// 状态码
    /// </summary>
    public uint Code { get; set; }

    /// <summary>
    /// 附加消息
    /// </summary>
    public string[] Messages
    {
        get => field ?? [];
        set => field = value;
    }

    /// <summary>
    /// 请求模型验证状态字典
    /// </summary>
    public IDictionary<string, string[]> ModelState
    {
        get => field ?? ImmutableDictionary<string, string[]>.Empty;
        set => field = value;
    }

    /// <summary>
    /// 调用的名称标识，例如请求地址或命令名称
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs#L58
    /// </summary>
    public string? TraceId { get; set; }

    public static implicit operator BMApiRsp(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator BMApiRsp(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator BMApiRsp(string message) => new() { Messages = [message], };

    public static implicit operator BMApiRsp(string[] messages) => new() { Messages = messages, };

    public static implicit operator BMApiRsp(Exception exception)
    {
        BMApiRsp result = new();
        result.SetException(exception);
        return result;
    }

    protected static void SetModelStateDictionary(BMApiRsp apiRsp, ModelStateDictionary modelState)
    {
        if (modelState.ErrorCount <= 0)
        {
            apiRsp.SetIsSuccess(true);
        }
        else
        {
            apiRsp.SetIsSuccess(false);

            string[] errorMessages = [.. modelState.Values
                .SelectMany(static x => x.Errors)
                .Select(static x=>x.ErrorMessage)
                .Where(static x => !string.IsNullOrWhiteSpace(x))];
            IDictionary<string, string[]> modelState2 = modelState
                .ToDictionary(
                    static x => x.Key,
                    static x => x.Value == null ? [] : x.Value.Errors.Select(static x => x.ErrorMessage).ToArray());
            apiRsp.Messages = errorMessages;
            apiRsp.ModelState = modelState2;
        }
    }

    public static implicit operator BMApiRsp(ModelStateDictionary dict)
    {
        BMApiRsp apiRsp = new();
        SetModelStateDictionary(apiRsp, dict);
        return apiRsp;
    }

    public static readonly BMApiRsp Ok = new()
    {
        Code = unchecked((uint)HttpStatusCode.OK),
    };

    public static implicit operator BMApiRsp(ApiRsp apiRsp) => new()
    {
        Code = apiRsp.Code,
        Messages = apiRsp.Message == null ? [] : [apiRsp.Message],
        Url = apiRsp.Url,
        TraceId = apiRsp.TraceId,
    };

    protected static IEnumerable<string> GetErrors(IdentityResult identityResult)
    {
        foreach (var error in identityResult.Errors)
            if (!string.IsNullOrWhiteSpace(error.Description))
                yield return error.Description;
            else if (!string.IsNullOrWhiteSpace(error.Code))
                yield return $"Identity Error, Code: {error.Code}";
    }

    public static implicit operator BMApiRsp(IdentityResult identityResult)
    {
        BMApiRsp r = new()
        {
            Messages = [.. GetErrors(identityResult)],
        };
        r.SetIsSuccess(false);
        return r;
    }
}

/// <summary>
/// 多租户管理后台的 WebApi 接口响应泛型模型
/// </summary>
/// <typeparam name="TContent"></typeparam>
public sealed partial record class BMApiRsp<TContent> : BMApiRsp
{
    /// <summary>
    /// 附加内容
    /// </summary>
    [JsonPropertyName("data")]
    public TContent? Content { get; set; }

    public static implicit operator BMApiRsp<TContent>(TContent content) => new() { Content = content };

    public static implicit operator BMApiRsp<TContent>(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator BMApiRsp<TContent>(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator BMApiRsp<TContent>(string message) => new() { Messages = [message], };

    public static implicit operator BMApiRsp<TContent>(string[] messages) => new() { Messages = messages, };

    public static implicit operator BMApiRsp<TContent>(Exception exception)
    {
        BMApiRsp<TContent> result = new();
        result.SetException(exception);
        return result;
    }

    public static implicit operator BMApiRsp<TContent>(ModelStateDictionary dict)
    {
        BMApiRsp<TContent> apiRsp = new();
        SetModelStateDictionary(apiRsp, dict);
        return apiRsp;
    }

    public static implicit operator BMApiRsp<TContent>(ApiRsp<TContent> apiRsp) => new()
    {
        Code = apiRsp.Code,
        Messages = apiRsp.Message == null ? [] : [apiRsp.Message],
        Url = apiRsp.Url,
        Content = apiRsp.Content,
        TraceId = apiRsp.TraceId,
    };

    public static implicit operator BMApiRsp<TContent>(IdentityResult identityResult)
    {
        BMApiRsp<TContent> r = new()
        {
            Messages = [.. GetErrors(identityResult)],
        };
        r.SetIsSuccess(false);
        return r;
    }

    public static new BMApiRsp<TContent> Ok => new()
    {
        Code = unchecked((uint)HttpStatusCode.OK),
    };
}
