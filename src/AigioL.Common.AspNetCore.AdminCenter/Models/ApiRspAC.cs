using AigioL.Common.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 多租户管理后台的 WebApi 接口响应模型
/// </summary>
public partial record class ApiRspAC
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => (Code >= 200u) && (Code <= 299u);

    /// <summary>
    /// 状态码
    /// </summary>
    public uint Code { get; set; }

    string[]? messages;

    /// <summary>
    /// 附加消息
    /// </summary>
    public string[] Messages
    {
        get => messages ?? [];
        set => messages = value;
    }

    KeyValuePair<string, string[]?>[]? modelState;

    /// <summary>
    /// 请求模型验证状态字典
    /// </summary>
    public KeyValuePair<string, string[]?>[] ModelState
    {
        get => modelState ?? [];
        set => modelState = value;
    }

    /// <summary>
    /// 调用的名称标识，例如请求地址或命令名称
    /// </summary>
    public string? Url { get; set; }

    public static implicit operator ApiRspAC(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator ApiRspAC(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator ApiRspAC(string message) => new() { Messages = [message], };

    public static implicit operator ApiRspAC(string[] messages) => new() { Messages = messages, };

    public static implicit operator ApiRspAC(Exception exception)
    {
        ApiRspAC result = new();
        result.SetException(exception);
        return result;
    }

    protected static void SetModelStateDictionary(ApiRspAC apiRsp, ModelStateDictionary modelState)
    {
        if (modelState.ErrorCount <= 0)
        {
            apiRsp.SetIsSuccess(true);
        }
        else
        {
            apiRsp.SetIsSuccess(false);
            apiRsp.messages = [.. modelState.Values.SelectMany(static x => x.Errors.Select(static x => x.ErrorMessage ?? string.Empty))];
            apiRsp.modelState = [.. modelState.Select(static x => new KeyValuePair<string, string[]?>(x.Key, x.Value == null ? null : [.. x.Value.Errors.Select(static e => e.ErrorMessage ?? string.Empty)]))];
        }
    }

    public static implicit operator ApiRspAC(ModelStateDictionary dict)
    {
        ApiRspAC apiRsp = new();
        SetModelStateDictionary(apiRsp, dict);
        return apiRsp;
    }

    public static readonly ApiRspAC Ok = new()
    {
        Code = unchecked((uint)HttpStatusCode.OK),
    };

    public static implicit operator ApiRspAC(ApiRsp apiRsp) => new()
    {
        Code = apiRsp.Code,
        Messages = apiRsp.Message == null ? [] : [apiRsp.Message],
        Url = apiRsp.Url,
    };
}

/// <summary>
/// 多租户管理后台的 WebApi 接口响应泛型模型
/// </summary>
/// <typeparam name="TContent"></typeparam>
public sealed partial record class ApiRspAC<TContent> : ApiRspAC
{
    /// <summary>
    /// 附加内容
    /// </summary>
    [JsonPropertyName("data")]
    public TContent? Content { get; set; }

    public static implicit operator ApiRspAC<TContent>(TContent content) => new() { Content = content };

    public static implicit operator ApiRspAC<TContent>(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator ApiRspAC<TContent>(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator ApiRspAC<TContent>(string message) => new() { Messages = [message], };

    public static implicit operator ApiRspAC<TContent>(string[] messages) => new() { Messages = messages, };

    public static implicit operator ApiRspAC<TContent>(Exception exception)
    {
        ApiRspAC<TContent> result = new();
        result.SetException(exception);
        return result;
    }

    public static implicit operator ApiRspAC<TContent>(ModelStateDictionary dict)
    {
        ApiRspAC<TContent> apiRsp = new();
        SetModelStateDictionary(apiRsp, dict);
        return apiRsp;
    }

    public static implicit operator ApiRspAC<TContent>(ApiRsp<TContent> apiRsp) => new()
    {
        Code = apiRsp.Code,
        Messages = apiRsp.Message == null ? [] : [apiRsp.Message],
        Url = apiRsp.Url,
        Content = apiRsp.Content,
    };

    public static new ApiRspAC<TContent> Ok => new()
    {
        Code = unchecked((uint)HttpStatusCode.OK),
    };
}
