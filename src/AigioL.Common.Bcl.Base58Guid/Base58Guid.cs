using SimpleBase;

namespace System;

/// <summary>
/// Base58 是比特币系统核心的编码方式，专为地址及私钥的可读性处理而设计。
/// 该编码从 Base64 改进而来，移除易混淆字符 0、O、I、l 及特殊符号 +/，采用 58 个字符构成字符集
/// </summary>
public static class Base58Guid
{
    public static string Encode(Guid guid)
    {
        Span<byte> b = stackalloc byte[16];
        guid.TryWriteBytes(b);
        return Base58.Bitcoin.Encode(b);
    }

    public static Guid? Decode(string? s)
    {
        if (s == null)
        {
            return null;
        }
        ReadOnlySpan<char> chars = s;
        return Decode(chars);
    }

    public static Guid? Decode(ReadOnlySpan<char> chars)
    {
        chars = chars.Trim();
        if (chars.Length == 0 || chars.Length > 30) // 一般长度为 22
        {
            return null;
        }
        Span<byte> b = stackalloc byte[16];
        if (Base58.Bitcoin.TryDecode(chars, b, out var bytesWritten) && bytesWritten == b.Length)
        {
            return new Guid(b);
        }
        return null;
    }
}