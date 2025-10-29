#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.AdminCenter.Models;

public static partial class ApiRspExtensions
{
    /// <summary>
    /// 给响应设置 HTTP 上下文信息，通常在返回之前末尾调用，以设置跟踪 Id 与请求地址
    /// </summary>
    public static TApiRspAC SetHttpContext<TApiRspAC>(this TApiRspAC apiRsp, HttpContext context) where TApiRspAC : BMApiRsp
    {
        if (string.IsNullOrWhiteSpace(apiRsp.Url))
        {
            apiRsp.Url = context.Request.Path;
        }
        if (string.IsNullOrWhiteSpace(apiRsp.TraceId))
        {
            apiRsp.TraceId = context.GetTraceId();
        }
        return apiRsp;
    }
}
