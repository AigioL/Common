#if !DISABLE_MP2
using MemoryPack;
#endif
using System.Net;
using System.Text.Json;

namespace AigioL.Common.Models;

#if !DISABLE_MP2
//#if MP2_GENERATE_TS
//[global::MemoryPack.GenerateTypeScript] 已手动修改生成代码
//#endif
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Sequential)]
#endif
public partial record class ApiRsp
{
    /// <summary>
    /// 是否成功
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess() => (Code >= 200u) && (Code <= 299u);

    /// <summary>
    /// 状态码
    /// </summary>
    public uint Code { get; set; }

    /// <summary>
    /// 附加消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 调用的名称标识，例如请求地址或命令名称
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// https://github.com/dotnet/aspnetcore/blob/v9.0.8/src/Http/Http.Extensions/src/DefaultProblemDetailsWriter.cs#L58
    /// </summary>
    public string? TraceId { get; set; }

    public static implicit operator ApiRsp(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator ApiRsp(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator ApiRsp(string message) => new() { Message = message };

    public static implicit operator ApiRsp(Exception exception)
    {
        ApiRsp result = new();
        result.SetException(exception);
        return result;
    }

    public static ApiRsp<TContent> Create<TContent>(TContent content)
    {
        var result = new ApiRsp<TContent> { Content = content };
        return result;
    }
}

#if !DISABLE_MP2
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Sequential)]
#endif
public sealed partial record class ApiRsp<TContent> : ApiRsp
{
    /// <summary>
    /// 附加内容
    /// </summary>
    public TContent? Content { get; set; }

    public static implicit operator ApiRsp<TContent>(TContent content) => new() { Content = content };

    public static implicit operator ApiRsp<TContent>(bool isSuccess) => isSuccess ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

    public static implicit operator ApiRsp<TContent>(HttpStatusCode statusCode) => new() { Code = unchecked((uint)statusCode) };

    public static implicit operator ApiRsp<TContent>(string message) => new() { Message = message };

    public static implicit operator ApiRsp<TContent>(Exception exception)
    {
        ApiRsp<TContent> result = new();
        result.SetException(exception);
        return result;
    }
}

partial record class ApiRsp // Code 常量
{
    #region 限定值范围 1008~1099

    /// <summary>
    /// 找不到 HTTP 请求授权头
    /// </summary>
    public const uint Code_MissingAuthHeader = 1008;

    /// <summary>
    /// HTTP 请求授权声明不正确
    /// </summary>
    public const uint Code_SchemeNotCorrect = 1009;

    /// <summary>
    /// 用户登录设备被踢出
    /// </summary>
    public const uint Code_UserDeviceIsNotTrust = 1010;

    /// <summary>
    /// 找不到用户
    /// </summary>
    public const uint Code_UserNotFound = 1011;

    /// <summary>
    /// 必须使用安全传输模式
    /// </summary>
    public const uint RequiredSecurityKey = 1017;

    /// <summary>
    /// 空的数据库 App 版本号
    /// </summary>
    public const uint EmptyDbAppVersion = 1022;

    /// <summary>
    /// RSA 解密失败或 16 进制字符串格式不正确
    /// </summary>
    public const uint RSADecryptFail = 1023;

    /// <summary>
    /// AES Key 不能为 null
    /// </summary>
    public const uint AesKeyIsNull = 1024;

    /// <summary>
    /// 加密类型和接口指定类型不一致
    /// </summary>
    public const uint SecurityTypeInconsistent = 1029;

    #endregion
}