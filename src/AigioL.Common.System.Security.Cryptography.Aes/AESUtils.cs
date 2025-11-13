using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Security.Cryptography;

public static partial class AESUtils
{
    /// <summary>
    /// 使用 GUID 作为 AES 密钥和 IV，密钥长度为 128 位，创建一个 <see cref="Aes"/> 实例
    /// </summary>
    /// <param name="keyIv"></param>
    /// <param name="bigEndian"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Aes Create(Guid keyIv, bool? bigEndian = null)
    {
        var aes = Aes.Create();
        aes.KeySize = 128; // 128 bits 时密钥与向量长度均为 16 字节等于一个 GUID

        bigEndian ??= BitConverter.IsLittleEndian;

        const int lenGuidBytes = 16;
        var buffer = ArrayPool<byte>.Shared.Rent(lenGuidBytes);
        try
        {
            keyIv.TryWriteBytes(buffer, bigEndian.Value, out var bytesWritten);
            if (bytesWritten != lenGuidBytes)
                throw new ArgumentException("Invalid GUID format", nameof(keyIv));
            aes.Key = aes.IV = buffer;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, true);
        }
        return aes;
    }

    const int MaxSizeInBytes = 50;

    static Aes? CreateCore(byte[] bytes)
    {
        switch (bytes.Length)
        {
            case 16: // AES-128 Key = IV = Guid
                {
                    var aes = Aes.Create();
                    aes.KeySize = 128; // 128 bits 时密钥与向量长度均为 16 字节等于一个 GUID
                    aes.Key = bytes;
                    aes.IV = bytes;
                    return aes;
                }
            case 18:
                {
                    var aes = Aes.Create();
                    aes.KeySize = 128;
                    var keyiv = bytes.AsSpan(2, 16).ToArray();
                    aes.Key = keyiv;
                    aes.IV = keyiv;
                    (var mode, var padding) = unchecked((Flags)BitConverter.ToUInt16(bytes.AsSpan(0, 2)));
                    aes.Mode = mode;
                    aes.Padding = padding;
                    return aes;
                }
            case 32:
                {
                    var aes = Aes.Create();
                    aes.KeySize = 128;
                    aes.IV = bytes.AsSpan(0, 16).ToArray();
                    aes.Key = bytes.AsSpan(16, 16).ToArray();
                    return aes;
                }
            case 34:
                {
                    var aes = Aes.Create();
                    aes.KeySize = 128;
                    aes.IV = bytes.AsSpan(2, 16).ToArray();
                    aes.Key = bytes.AsSpan(18, 16).ToArray();
                    (var mode, var padding) = unchecked((Flags)BitConverter.ToUInt16(bytes.AsSpan(0, 2)));
                    aes.Mode = mode;
                    aes.Padding = padding;
                    return aes;
                }
            case 40:
                {
                    var aes = Aes.Create();
                    aes.KeySize = 192;
                    aes.IV = bytes.AsSpan(0, 16).ToArray();
                    aes.Key = bytes.AsSpan(16, 24).ToArray();
                    return aes;
                }
            case 42:
                {
                    var aes = Aes.Create();
                    aes.KeySize = 192;
                    aes.IV = bytes.AsSpan(2, 16).ToArray();
                    aes.Key = bytes.AsSpan(18, 24).ToArray();
                    (var mode, var padding) = unchecked((Flags)BitConverter.ToUInt16(bytes.AsSpan(0, 2)));
                    aes.Mode = mode;
                    aes.Padding = padding;
                    return aes;
                }
            case 48:
                {
                    var aes = Aes.Create();
                    aes.KeySize = 256;
                    aes.IV = bytes.AsSpan(0, 16).ToArray();
                    aes.Key = bytes.AsSpan(16, 32).ToArray();
                    return aes;
                }
            case 50:
                {
                    var aes = Aes.Create();
                    aes.KeySize = 256;
                    aes.IV = bytes.AsSpan(2, 16).ToArray();
                    aes.Key = bytes.AsSpan(16, 32).ToArray();
                    (var mode, var padding) = unchecked((Flags)BitConverter.ToUInt16(bytes.AsSpan(0, 2)));
                    aes.Mode = mode;
                    aes.Padding = padding;
                    return aes;
                }
            default:
                return null;
        }
    }

    public static Aes? Create(byte[] bytes)
    {
        if (bytes.Length > MaxSizeInBytes)
        {
            // 某些小程序平台不支持 byte 格式传递值，使用 Hex 字符串传递
            var maxBytes = Encoding.UTF8.GetMaxByteCount(100);
            if (bytes.Length <= maxBytes)
            {
                var len = Encoding.UTF8.GetMaxCharCount(bytes.Length);
                Span<char> chars = stackalloc char[len];
                len = Encoding.UTF8.GetChars(bytes, chars);
                chars = chars[..len];
                var bytes2 = Convert.FromHexString(chars);
                if (bytes2.Length <= MaxSizeInBytes)
                {
                    return CreateCore(bytes2);
                }
            }
        }
        else
        {
            return CreateCore(bytes);
        }
        return null;

    }

    public static Aes? CreateOld(byte[] data)
    {
        //var uint16 = BitConverter.ToUInt16(data, 0);
        //var flags = (Flags)uint16;
        var mIVByteArray = data.Skip(2).Take(16).ToArray();
        var mKeyByteArray = data.Skip(18).Reverse().ToArray();
        var aes = Aes.Create();
        aes.Key = mKeyByteArray;
        aes.IV = mIVByteArray;
        //aes.Mode = Mode;
        //aes.Padding = Padding;
        return aes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFlags(Aes aes, Flags flags)
    {
        (var mode, var padding) = flags;
        aes.Mode = mode;
        aes.Padding = padding;
    }

    public static void Deconstruct(this Flags flags, out CipherMode mode, out PaddingMode padding)
    {
#pragma warning disable CS0618 // 类型或成员已过时
        if ((flags & Flags.CipherMode_CBC) == Flags.CipherMode_CBC)
            mode = CipherMode.CBC;
        else if ((flags & Flags.CipherMode_CTS) == Flags.CipherMode_CTS)
            mode = CipherMode.CTS;
        else if ((flags & Flags.CipherMode_OFB) == Flags.CipherMode_OFB)
            mode = CipherMode.OFB;
        else if ((flags & Flags.CipherMode_CFB) == Flags.CipherMode_CFB)
            mode = CipherMode.CFB;
        else if ((flags & Flags.CipherMode_ECB) == Flags.CipherMode_ECB)
            mode = CipherMode.ECB;
        else
            mode = CipherMode.CBC;

        if ((flags & Flags.PaddingMode_PKCS7) == Flags.PaddingMode_PKCS7)
            padding = PaddingMode.PKCS7;
        else if ((flags & Flags.PaddingMode_Zeros) == Flags.PaddingMode_Zeros)
            padding = PaddingMode.Zeros;
        else if ((flags & Flags.PaddingMode_ANSIX923) == Flags.PaddingMode_ANSIX923)
            padding = PaddingMode.ANSIX923;
        else if ((flags & Flags.PaddingMode_ISO10126) == Flags.PaddingMode_ISO10126)
            padding = PaddingMode.ISO10126;
        else if ((flags & Flags.PaddingMode_None) == Flags.PaddingMode_None)
            padding = PaddingMode.None;
        else
            padding = PaddingMode.PKCS7;
#pragma warning restore CS0618 // 类型或成员已过时
    }

    public static Flags GetFlags(CipherMode mode, PaddingMode padding)
    {
#pragma warning disable CS0618 // 类型或成员已过时
        Flags flags = default;

        switch (mode)
        {
            case CipherMode.CBC:
                flags &= Flags.CipherMode_CBC;
                break;
            case CipherMode.ECB:
                flags &= Flags.CipherMode_ECB;
                break;
            case CipherMode.OFB:
                flags &= Flags.CipherMode_OFB;
                break;
            case CipherMode.CFB:
                flags &= Flags.CipherMode_CFB;
                break;
            case CipherMode.CTS:
                flags &= Flags.CipherMode_CTS;
                break;
        }

        switch (padding)
        {
            case PaddingMode.None:
                flags &= Flags.PaddingMode_None;
                break;
            case PaddingMode.PKCS7:
                flags &= Flags.PaddingMode_PKCS7;
                break;
            case PaddingMode.Zeros:
                flags &= Flags.PaddingMode_Zeros;
                break;
            case PaddingMode.ANSIX923:
                flags &= Flags.PaddingMode_ANSIX923;
                break;
            case PaddingMode.ISO10126:
                flags &= Flags.PaddingMode_ISO10126;
                break;
        }

        return flags;
#pragma warning restore CS0618 // 类型或成员已过时
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Flags GetFlags(Aes aes) => GetFlags(aes.Mode, aes.Padding);

    /// <summary>
    /// 加密模式，填充枚举
    /// </summary>
    [Flags]
    public enum Flags : ushort
    {
        CipherMode_CBC = 2,

        /// <summary>
        /// ECB模式已被定义为不安全的，除非对接第三方服务要求使用之外，否则不应使用！
        /// </summary>
        [Obsolete("low safety.")]
        CipherMode_ECB = 16,

        //[Obsolete("target .NET Standard 1.4 is not supported.")]
        CipherMode_OFB = 32,

        //[Obsolete("target .NET Standard 1.4 is not supported.")]
        CipherMode_CFB = 64,

        CipherMode_CTS = 512,
        PaddingMode_None = 4096,
        PaddingMode_PKCS7 = 8,
        PaddingMode_Zeros = 1024,

        //[Obsolete("target .NET Standard 1.4 is not supported.")]
        PaddingMode_ANSIX923 = 2048,

        //[Obsolete("target .NET Standard 1.4 is not supported.")]
        PaddingMode_ISO10126 = 4,
    }
}
