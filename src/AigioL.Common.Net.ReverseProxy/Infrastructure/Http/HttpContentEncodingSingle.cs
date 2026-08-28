#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// HTTP 头的 Content-Encoding 单值枚举，按优先级顺序排列，值大的优先使用
/// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Reference/Headers/Content-Encoding</para>
/// </summary>
public enum HttpContentEncodingSingle : byte
{
    /// <summary>
    /// 无值
    /// </summary>
    Null = 0,

    /// <summary>
    /// 采用 zlib 结构（在 RFC 1950 中规定）和 deflate 压缩算法（在 RFC 1951 中规定)
    /// </summary>
    Deflate = 1,

    /// <summary>
    /// 采用 Lempel-Ziv-Welch（LZW）压缩算法。这个名称来自 UNIX 系统的 compress 程序，该程序实现了前述算法。与其同名程序已经在大部分 UNIX 发行版中消失一样，这种内容编码方式已经被大部分浏览器弃用，部分因为专利问题（这项专利在 2003 年到期）
    /// </summary>
    Compress = 6,

    /// <summary>
    /// 表示采用 Lempel-Ziv coding（LZ77）压缩算法，以及 32 位 CRC 校验的编码方式。这个编码方式最初由 UNIX 平台上的 gzip 程序采用。出于兼容性的考虑，HTTP/1.1 标准提议支持这种编码方式的服务器应该识别作为别名的 x-gzip 指令
    /// </summary>
    Gzip = 11,

    /// <summary>
    /// 采用 Brotli 算法结构（在 RFC 7932 中规定）的格式
    /// </summary>
    Br = 16,

    /// <summary>
    /// 采用 Zstandard 算法结构（在 RFC 8878 中规定）的格式
    /// </summary>
    Zstd = 21,
}
