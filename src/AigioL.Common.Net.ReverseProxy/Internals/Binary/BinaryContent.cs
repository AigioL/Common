using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace AigioL.Common.Net.ReverseProxy.Internals.Binary;

/// <summary>
/// 二进制内容
/// </summary>
sealed class BinaryContent
{
    readonly IFileInfo? fileInfo;
    readonly ReadOnlyMemory<byte> utf8Bytes;
    readonly BinaryContentType contentType;

    public BinaryContent()
    {
    }

    public BinaryContent(IFileInfo fileInfo)
    {
        this.fileInfo = fileInfo;
        contentType = BinaryContentType.File;
    }

    public BinaryContent(ReadOnlyMemory<byte> utf8Bytes)
    {
        this.utf8Bytes = utf8Bytes;
        contentType = BinaryContentType.Memory;
    }

    public static implicit operator BinaryContent(PhysicalFileInfo fileInfo) => new(fileInfo);

    public static implicit operator BinaryContent(FileInfo fileInfo) => new(new PhysicalFileInfo(fileInfo));

    public static implicit operator BinaryContent(ReadOnlyMemory<byte> utf8Bytes) => new(utf8Bytes);

    /// <summary>
    /// 将二进制内容写入响应流
    /// </summary>
    public async Task WriteAsync(HttpResponse rsp, CancellationToken cancellationToken = default)
    {
        switch (contentType)
        {
            case BinaryContentType.File:
                ArgumentNullException.ThrowIfNull(fileInfo);
                await rsp.SendFileAsync(fileInfo, cancellationToken);
                break;
            case BinaryContentType.Memory:
                await rsp.BodyWriter.WriteAsync(utf8Bytes, cancellationToken);
                break;
        }
    }
}
