using AigioL.Common.Net.ReverseProxy.Internals.Binary;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

static partial class BinaryContentExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteAsync(
        this HttpResponse rsp,
        BinaryContent content,
        CancellationToken cancellationToken = default)
            => content.WriteAsync(rsp, cancellationToken);
}