using Microsoft.AspNetCore.Http;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Binary;

/// <summary>
/// 二进制内容类型
/// </summary>
enum BinaryContentType : byte
{
    /// <summary>
    /// 无内容，空的
    /// </summary>
    None,

    /// <summary>
    /// 内容来自文件
    /// </summary>
    File,

    /// <summary>
    /// 内容来自内存中的 <see cref="ReadOnlyMemory{T}"/> 或 <see cref="Array"/>
    /// </summary>
    Memory,

    /// <summary>
    /// 内容来自 <see cref="IResult"/>
    /// </summary>
    HttpResult,
}