using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Globalization;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Http;

/// <summary>
/// https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Abstractions/src/HostString.cs
/// </summary>
static class HostString2
{
#if !NETFRAMEWORK
    /// <summary>
    /// https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Abstractions/src/HostString.cs#L24
    /// </summary>
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "s_safeHostStringChars")]
    extern static ref SearchValues<char> GetSafeHostStringChars(HostString _ = default);

    /// <summary>
    /// https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Abstractions/src/HostString.cs#L27
    /// </summary>
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "s_idnMapping")]
    extern static ref IdnMapping GetIdnMapping(HostString _ = default);

    /// <summary>
    /// Parses the current value. IPv6 addresses will have brackets added if they are missing.
    /// </summary>
    /// <param name="value">The value to get the parts of.</param>
    /// <param name="host">The portion of the <paramref name="value"/> which represents the host.</param>
    /// <param name="port">The portion of the <paramref name="value"/> which represents the port.</param>
    static void GetParts(StringSegment value, out StringSegment host, out StringSegment port)
    {
        // https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Abstractions/src/HostString.cs#L350
        int index;
        port = null;
        host = null;

        if (StringSegment.IsNullOrEmpty(value))
        {
            return;
        }
        else if ((index = value.IndexOf(']')) >= 0)
        {
            // IPv6 in brackets [::1], maybe with port
            host = value.Subsegment(0, index + 1);
            // Is there a colon and at least one character?
            if (index + 2 < value.Length && value[index + 1] == ':')
            {
                port = value.Subsegment(index + 2);
            }
        }
        else if ((index = value.IndexOf(':')) >= 0
            && index < value.Length - 1
            && value.IndexOf(':', index + 1) >= 0)
        {
            // IPv6 without brackets ::1 is the only type of host with 2 or more colons
            host = $"[{value}]";
            port = null;
        }
        else if (index >= 0)
        {
            // Has a port
            host = value.Subsegment(0, index);
            port = value.Subsegment(index + 1);
        }
        else
        {
            host = value;
            port = null;
        }
    }
#endif

    /// <summary>
    /// 将 <see cref="HostString"/> 写入 <see cref="PipeWriter"/>
    /// </summary>
    public static void Write(this IBufferWriter<byte> writer, HostString value)
    {
#if NETFRAMEWORK
        writer.Write(Encoding.UTF8.GetBytes(value.ToString()));
#else
        // https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Http/Http.Abstractions/src/HostString.cs#L125-L154
        if (!value.HasValue)
        {
            return;
        }

        var valSpan = value.Value.AsSpan();
        var s_safeHostStringChars = GetSafeHostStringChars();
        if (!valSpan.ContainsAnyExcept(s_safeHostStringChars))
        {
            writer.Write(valSpan);
        }

        GetParts(value.Value, out var host, out var port);

        var s_idnMapping = GetIdnMapping();
        var unicode = host.Buffer!.AsSpan(host.Offset, host.Length);

        char[]? array = null;
        Span<char> chars = unicode.Length <= StackallocCharThreshold ?
            stackalloc char[StackallocCharThreshold] :
            (array = ArrayPool<char>.Shared.Rent(unicode.Length)).AsSpan(0, unicode.Length);
        try
        {
            if (s_idnMapping.TryGetAscii(unicode, chars, out var charsWritten))
            {
                var encoded = chars[..charsWritten];
                if (StringSegment.IsNullOrEmpty(port))
                {
                    writer.Write(encoded);
                }
                else
                {
                    writer.Write(encoded);
                    writer.Write(':');
                    writer.Write(port.AsSpan());
                }
            }
            else
            {
                var encoded = s_idnMapping.GetAscii(host.Buffer!, host.Offset, host.Length);
                if (StringSegment.IsNullOrEmpty(port))
                {
                    writer.Write(encoded);
                }
                else
                {
                    writer.Write(encoded);
                    writer.Write(':');
                    writer.Write(port.AsSpan());
                }
            }
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<char>.Shared.Return(array);
            }
        }
#endif
    }
}
