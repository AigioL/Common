using Microsoft.IO;
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Web;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Security.Cryptography;

public static partial class RSAExtensions
{
    /// <summary>
    /// 用于 JS 前端数据的 RSA 解密
    /// </summary>
    /// <param name="rsa"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string BMDecrypt(this RSA rsa, ReadOnlySpan<char> text)
    {
        ReadOnlySpan<char> separator = [' ', '\r', '\n'];
        var split = text.Split(separator);

        using var stream = _.m.GetStream();
        foreach (var range in split)
        {
            var it = text[range];
            if (it.IsWhiteSpace())
            {
                continue;
            }
            var data = _.DecryptHexStringToBytes(rsa, it, RSAEncryptionPadding.Pkcs1);
            stream.Write(data);
        }

        var buffer = stream.GetBuffer();
        var result = HttpUtility.UrlDecode(buffer, 0, unchecked((int)stream.Length), Encoding.UTF8);
        return result;
    }

    /// <summary>
    /// 用于 JS 前端数据的 RSA 解密
    /// </summary>
    /// <param name="rsa"></param>
    /// <param name="text">前端分割的 RSA 加密</param>
    /// <returns></returns>
    public static string BMDecryptBase64(this RSA rsa, ReadOnlySpan<char> text)
    {
        ReadOnlySpan<char> separator = [' ', '\r', '\n'];
        var split = text.Split(separator);

        using var stream = _.m.GetStream();
        foreach (var range in split)
        {
            var it = text[range];
            if (it.IsWhiteSpace())
            {
                continue;
            }

            var len = (it.Length / 4) * 3;
            var b = ArrayPool<byte>.Shared.Rent(len);
            try
            {
                if (!Convert.TryFromBase64Chars(it, b, out var bytesWritten))
                {
                    throw new FormatException($"无法将 Base64 字符串转换为字节数组: {it.ToString()}");
                }
                var writtenSpan = b.AsSpan(0, bytesWritten);
                writtenSpan = rsa.Decrypt(writtenSpan, RSAEncryptionPadding.Pkcs1);
                stream.Write(writtenSpan);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(b);
            }
        }

        var buffer = stream.GetBuffer();
        var result = HttpUtility.UrlDecode(buffer, 0, unchecked((int)stream.Length), Encoding.UTF8);
        return result;
    }
}

file static class _
{
    internal static readonly RecyclableMemoryStreamManager m = new();

    /// <summary>
    /// 16 进制字符串转 Bytes
    /// </summary>
    /// <param name="rsa"></param>
    /// <param name="hex"></param>
    /// <param name="padding"></param>
    /// <returns></returns>
    internal static byte[] DecryptHexStringToBytes(RSA rsa, ReadOnlySpan<char> hex, RSAEncryptionPadding padding)
    {
        if (hex.Length == 0)
        {
            var result = rsa.Decrypt(stackalloc byte[1], padding);
            return result;
        }
        char[]? chars = null;
        try
        {
            if (hex.Length % 2 == 1)
            {
                var len = hex.Length + 1;
                chars = ArrayPool<char>.Shared.Rent(len);
                chars[0] = '0'; // 前面补零
                hex.CopyTo(chars.AsSpan(1));
                hex = chars.AsSpan(0, len);
            }
            var lenB = hex.Length / 2;
            byte[] numArray = ArrayPool<byte>.Shared.Rent(hex.Length / 2);
            try
            {
                for (var index = 0; index < hex.Length / 2; ++index)
                {
                    numArray[index] = byte.Parse(hex.Slice(2 * index, 2), NumberStyles.AllowHexSpecifier);
                }
                var result = rsa.Decrypt(numArray.AsSpan(0, lenB), padding);
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(numArray, true);
            }
        }
        finally
        {
            if (chars != null)
                ArrayPool<char>.Shared.Return(chars, true);
        }
    }
}