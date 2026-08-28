using AigioL.Common.Net.ReverseProxy.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System.Buffers;
using System.Globalization;
using System.Text;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

namespace AigioL.Common.Net.ReverseProxy.Infrastructure.Binary;

/// <summary>
/// 脚本注入助手类
/// </summary>
static class ScriptInjectHelper
{
    /// <summary>
    /// 从 <see cref="RecyclableMemoryStream"/> 中查找脚本注入位置
    /// </summary>
    internal static bool FindScriptInjectInsertPosition(RecyclableMemoryStream stream, Encoding encoding, out int insertPosition)
    {
        var seq = stream.GetReadOnlySequence();
        if (seq.IsEmpty || seq.Length > int.MaxValue)
        {
            insertPosition = default;
            return false;
        }
        if (seq.IsSingleSegment)
        {
            return FindScriptInjectInsertPosition(seq.FirstSpan, encoding, out insertPosition);
        }
        else
        {
            // TODO: 将查找算法实现由 ReadOnlySpan<byte> 改成 ReadOnlySequence<byte> 减少分配
            var expectedByteCount = unchecked((int)seq.Length);
            byte[]? array = null;
            Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
                stackalloc byte[StackallocByteThreshold] :
                (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
            try
            {
                seq.CopyTo(utf8Bytes);
                return FindScriptInjectInsertPosition(utf8Bytes, encoding, out insertPosition);
            }
            finally
            {
                if (array is not null)
                {
                    ArrayPool<byte>.Shared.Return(array);
                }
            }
        }
    }

    /// <summary>
    /// 从 <see cref="RecyclableMemoryStream"/> 中查找脚本注入位置（GitHub）
    /// </summary>
    internal static bool FindScriptInjectInsertPositionForGitHub(RecyclableMemoryStream stream, Encoding encoding, out int insertPosition)
    {
        var seq = stream.GetReadOnlySequence();
        if (seq.IsEmpty || seq.Length > int.MaxValue)
        {
            insertPosition = default;
            return false;
        }
        if (seq.IsSingleSegment)
        {
            return FindScriptInjectInsertPositionForGitHub(seq.FirstSpan, encoding, out insertPosition);
        }
        else
        {
            // TODO: 将查找算法实现由 ReadOnlySpan<byte> 改成 ReadOnlySequence<byte> 减少分配
            var expectedByteCount = unchecked((int)seq.Length);
            byte[]? array = null;
            Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
                stackalloc byte[StackallocByteThreshold] :
                (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
            try
            {
                seq.CopyTo(utf8Bytes);
                return FindScriptInjectInsertPositionForGitHub(utf8Bytes, encoding, out insertPosition);
            }
            finally
            {
                if (array is not null)
                {
                    ArrayPool<byte>.Shared.Return(array);
                }
            }
        }
    }

    /// <summary>
    /// 判断 HOST 是否为 GitHub
    /// </summary>
    internal static bool IsGitHubHost(HostString host)
    {
        if (!host.HasValue)
        {
            return false;
        }

        var r = host.Host.Equals("github.com", StringComparison.InvariantCultureIgnoreCase) ||
            host.Host.EndsWith(".github.com", StringComparison.InvariantCultureIgnoreCase);
        return r;
    }

    /// <summary>
    /// 将 <see langword="int"/> 以 UTF-8 编码写入流，不分配字符串
    /// </summary>
    static void WriteInt32(Stream s, int value)
    {
        Span<byte> buffer = stackalloc byte[11];
        bool result = value.TryFormat(buffer, out int bytesWritten, default, CultureInfo.InvariantCulture);
        if (result)
        {
            buffer = buffer[..bytesWritten];
            s.Write(buffer);
        }
        else
        {
            s.Write(Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture)));
        }
    }

    /// <summary>
    /// 将 <see cref="ReadOnlySequence{T}"/> 异步写入 <see cref="Stream"/> 中
    /// </summary>
    static async ValueTask WriteAsync(Stream s, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken = default)
    {
        foreach (var it in buffer)
        {
            await s.WriteAsync(it, cancellationToken);
        }
    }

    /// <summary>
    /// 写入注入的脚本数据的 HTML
    /// </summary>
    internal static async ValueTask WriteUtf8HtmlAsync(Stream s, int position, ReadOnlySequence<byte> buffer, IEnumerable<IScriptConfig> scripts, CancellationToken cancellationToken = default)
    {
        // html 前部分数据
        var html_start = buffer.Slice(0, position);
        await WriteAsync(s, html_start, cancellationToken);

        // 中间插入 js 脚本
        var script_xml_start = "<script type=\"text/javascript\" src=\""u8;
        var script_xml_end = ".js\"></script>\r\n"u8;
        foreach (var script in scripts)
        {
            s.Write(script_xml_start);
            WriteInt32(s, script.LocalId);
            s.Write(script_xml_end);
        }

        // html 后部分数据
        var html_end = buffer.Slice(position);
        await WriteAsync(s, html_end, cancellationToken);
    }

    /// <summary>
    /// 从 <see cref="ReadOnlySpan{T}"/> 中查找脚本注入位置
    /// </summary>
    internal static bool FindScriptInjectInsertPosition(ReadOnlySpan<byte> buffer, Encoding encoding, out int insertPosition)
    {
        // 匹配 </...> 60 47 ... 62
        ReadOnlySpan<byte> mark_start = "</"u8;
        ReadOnlySpan<byte> mark_end = ">"u8;
        if (mark_start.Length <= 0 || mark_end.Length <= 0) goto notfound;

        int index_name_end = 0;
        int match_mark_end_index = 0;
        int match_mark_start_index = 0;

        for (int i = buffer.Length - 1; i >= 0; i--) // 倒序匹配，对应之前的 LastIndexOf(string
        {
            var item = buffer[i];
            if (index_name_end == 0)
            {
                var index = mark_end.Length - 1 - match_mark_end_index;
                if (index >= 0 && index < mark_end.Length && item == mark_end[index]) // 匹配末尾
                {
                    if (item == mark_end[index])
                    {
                        match_mark_end_index++;
                        if (match_mark_end_index >= mark_end.Length)
                        {
                            if (index_name_end == 0)
                            {
                                index_name_end = i;
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                var index = mark_start.Length - 1 - match_mark_start_index;
                if (index >= 0 && index < mark_start.Length && item == mark_start[index]) // 匹配开头
                {
                    match_mark_start_index++;
                    if (match_mark_start_index >= mark_start.Length)
                    {
                        const int matchCharCount = 4;
                        var index_name_start = i + mark_start.Length;
                        //if (encoding.GetMaxCharCount(index_name_end - index_name_start) >= matchCharCount)
                        //{
                        var bytes = buffer[index_name_start..index_name_end];
                        var charCount = encoding.GetCharCount(bytes);
                        if (charCount == matchCharCount)
                        {
                            var body = "BODY"u8;
                            var head = "HEAD"u8;
                            var comparer = Utf8StringComparerOrdinalIgnoreCase.Instance;
                            if ((bytes.Length == body.Length &&
                                bytes.SequenceEqual(body, comparer)) ||
                                (bytes.Length == head.Length &&
                                bytes.SequenceEqual(head, comparer)))
                            {
                                insertPosition = index_name_start - mark_start.Length;
                                return true;
                            }
                        }
                        //}
                        goto reset;
                    }
                }
            }

            continue;

        reset: index_name_end = match_mark_end_index = match_mark_start_index = 0;
        }

    notfound: insertPosition = -1;
        return false;
    }

    /// <summary>
    /// 从 <see cref="ReadOnlySpan{T}"/> 中查找脚本注入位置（GitHub）
    /// </summary>
    internal static bool FindScriptInjectInsertPositionForGitHub(ReadOnlySpan<byte> buffer, Encoding encoding, out int insertPosition)
    {
        ReadOnlySpan<byte> mark = "<script"u8;
        var lastScriptWithSrcStart = -1;

        if (buffer.Length >= mark.Length)
        {
            for (var i = 0; i <= buffer.Length - mark.Length; i++)
            {
                if (!EqualsAsciiIgnoreCase(buffer.Slice(i, mark.Length), mark))
                {
                    continue;
                }

                var afterMarkIndex = i + mark.Length;
                if (afterMarkIndex < buffer.Length && IsHtmlAttributeNameChar(buffer[afterMarkIndex]))
                {
                    continue;
                }

                var tagEndOffset = buffer[afterMarkIndex..].IndexOf(unchecked((byte)'>'));
                if (tagEndOffset < 0)
                {
                    break;
                }

                var tagEndIndex = afterMarkIndex + tagEndOffset;
                var scriptTag = buffer.Slice(i, tagEndIndex - i + 1);
                if (ScriptTagHasSrcAttribute(scriptTag))
                {
                    lastScriptWithSrcStart = i;
                }

                i = tagEndIndex;
            }
        }

        if (lastScriptWithSrcStart >= 0)
        {
            insertPosition = lastScriptWithSrcStart;
            return true;
        }

        return FindScriptInjectInsertPosition(buffer, encoding, out insertPosition);
    }

    static bool ScriptTagHasSrcAttribute(ReadOnlySpan<byte> scriptTag)
    {
        ReadOnlySpan<byte> src = "src"u8;

        if (scriptTag.Length < src.Length)
        {
            return false;
        }

        for (var i = 0; i <= scriptTag.Length - src.Length; i++)
        {
            if (!EqualsAsciiIgnoreCase(scriptTag.Slice(i, src.Length), src))
            {
                continue;
            }

            if (i > 0 && IsHtmlAttributeNameChar(scriptTag[i - 1]))
            {
                continue;
            }

            var j = i + src.Length;
            while (j < scriptTag.Length && IsAsciiWhitespace(scriptTag[j]))
            {
                j++;
            }

            if (j < scriptTag.Length && scriptTag[j] == unchecked((byte)'='))
            {
                return true;
            }
        }

        return false;
    }

    static bool EqualsAsciiIgnoreCase(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            var l = ToUpperAscii(left[i]);
            var r = ToUpperAscii(right[i]);
            if (l != r)
            {
                return false;
            }
        }

        return true;
    }

    static byte ToUpperAscii(byte value)
    {
        if (value is >= (byte)'a' and <= (byte)'z')
            return (byte)(value - 32);
        return value;
    }

    static bool IsAsciiWhitespace(byte value) =>
        value == unchecked((byte)' ') ||
        value == unchecked((byte)'\t') ||
        value == unchecked((byte)'\r') ||
        value == unchecked((byte)'\n') ||
        value == unchecked((byte)'\f');

    static bool IsHtmlAttributeNameChar(byte value) =>
        (value is >= unchecked((byte)'a') and <= unchecked((byte)'z')) ||
        (value is >= unchecked((byte)'A') and <= unchecked((byte)'Z')) ||
        (value is >= unchecked((byte)'0') and <= unchecked((byte)'9')) ||
        value == unchecked((byte)'-') ||
        value == unchecked((byte)'_') ||
        value == unchecked((byte)':') ||
        value == unchecked((byte)'.');
}
