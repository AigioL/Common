using System.Runtime.Versioning;

namespace System.Security.Cryptography;

public enum HashAlgorithmTypeName : byte
{
    MD5 = 1,
    SHA1,

    SHA256,
    SHA384,
    SHA512,

    [SupportedOSPlatform("windows10.0.25324")]
    SHA3_256,
    [SupportedOSPlatform("windows10.0.25324")]
    SHA3_384,
    [SupportedOSPlatform("windows10.0.25324")]
    SHA3_512,

    Crc32,
    Crc64,
}

public static partial class HashAlgorithmTypeNameEnumExtensions
{
    /// <summary>
    /// 根据哈希算法类型名称获取 HMAC 哈希值的字节长度
    /// </summary>
    public static int GetHMACHashSizeInBytes(
        this HashAlgorithmTypeName hashAlgorithmTypeName)
    {
        var hashSizeInBytes = hashAlgorithmTypeName switch
        {
            HashAlgorithmTypeName.SHA512 => HMACSHA512.HashSizeInBytes,
            HashAlgorithmTypeName.SHA384 => HMACSHA384.HashSizeInBytes,
            HashAlgorithmTypeName.SHA256 => HMACSHA256.HashSizeInBytes,
            HashAlgorithmTypeName.SHA1 => HMACSHA1.HashSizeInBytes,
            HashAlgorithmTypeName.MD5 => HMACMD5.HashSizeInBytes,
            _ => throw new ArgumentOutOfRangeException(
                nameof(hashAlgorithmTypeName),
                hashAlgorithmTypeName,
                "Unsupported HMAC hash algorithm type name."),
        };
        return hashSizeInBytes;
    }

    /// <summary>
    /// 根据哈希算法类型名称计算 HMAC 哈希值
    /// </summary>
    public static ValueTask<int> HMACHashDataAsync(
        this HashAlgorithmTypeName hashAlgorithmTypeName,
        ReadOnlyMemory<byte> key,
        Stream? source,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        source ??= new MemoryStream([], false); // 空流不需要释放

        var len = hashAlgorithmTypeName switch
        {
            HashAlgorithmTypeName.SHA512 => HMACSHA512.HashDataAsync(key, source, destination, cancellationToken),
            HashAlgorithmTypeName.SHA384 => HMACSHA384.HashDataAsync(key, source, destination, cancellationToken),
            HashAlgorithmTypeName.SHA256 => HMACSHA256.HashDataAsync(key, source, destination, cancellationToken),
            HashAlgorithmTypeName.SHA1 => HMACSHA1.HashDataAsync(key, source, destination, cancellationToken),
            HashAlgorithmTypeName.MD5 => HMACMD5.HashDataAsync(key, source, destination, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(hashAlgorithmTypeName),
                hashAlgorithmTypeName,
                "Unsupported HMAC hash algorithm type name."),
        };
        return len;
    }

    public static ValueTask<int> HashDataAsync(
        this HashAlgorithmTypeName hashAlgorithmTypeName,
        Stream? source,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        source ??= new MemoryStream([], false); // 空流不需要释放

        var len = hashAlgorithmTypeName switch
        {
            HashAlgorithmTypeName.SHA512 => SHA512.HashDataAsync(source, destination, cancellationToken),
            HashAlgorithmTypeName.SHA384 => SHA384.HashDataAsync(source, destination, cancellationToken),
            HashAlgorithmTypeName.SHA256 => SHA256.HashDataAsync(source, destination, cancellationToken),
            HashAlgorithmTypeName.SHA1 => SHA1.HashDataAsync(source, destination, cancellationToken),
            HashAlgorithmTypeName.MD5 => MD5.HashDataAsync(source, destination, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(
                nameof(hashAlgorithmTypeName),
                hashAlgorithmTypeName,
                "Unsupported hash algorithm type name."),
        };
        return len;
    }

    /// <summary>
    /// 将请求正文通过 <see cref="HMAC"/> 哈希算法进行哈希计算，并将结果写入 destination 中，如果传递了 destinationChars 缓冲区，则将哈希结果转换为小写十六进制字符串并写入 destinationChars 中
    /// </summary>
    /// <param name="hashAlgorithmTypeName"></param>
    /// <param name="key">HMAC-SHA256 哈希算法的密钥</param>
    /// <param name="source">要进行哈希计算的源数据流</param>
    /// <param name="destination">用于存储哈希结果的目标缓冲区</param>
    /// <param name="destinationChars">用于存储哈希结果的十六进制字符串的目标缓冲区</param>
    /// <param name="lower">指示是否将哈希结果转换为小写十六进制字符串</param>
    /// <param name="cancellationToken">用于取消操作的令牌</param>
    /// <returns></returns>
    public static async ValueTask HMACHashDataAsync(
        this HashAlgorithmTypeName hashAlgorithmTypeName,
        ReadOnlyMemory<byte> key,
        Stream? source,
        Memory<byte> destination,
        Memory<char> destinationChars = default,
        bool lower = true,
        CancellationToken cancellationToken = default)
    {
        var len = await hashAlgorithmTypeName.HMACHashDataAsync(key, source, destination, cancellationToken);
        destination = destination[..len];
        if (!destinationChars.IsEmpty)
        {
            var hashSizeInBytes = hashAlgorithmTypeName.GetHMACHashSizeInBytes();
            if (destinationChars.Length >= hashSizeInBytes * 2)
            {
                if (lower)
                    Convert.TryToHexStringLower(destination.Span, destinationChars.Span, out var _);
                else
                    Convert.TryToHexString(destination.Span, destinationChars.Span, out var _);
            }
        }
    }

    public static async ValueTask HashDataAsync(
        this HashAlgorithmTypeName hashAlgorithmTypeName,
        Stream? source,
        Memory<byte> destination,
        Memory<char> destinationChars = default,
        bool lower = true,
        CancellationToken cancellationToken = default)
    {
        var len = await hashAlgorithmTypeName.HashDataAsync(source, destination, cancellationToken);
        destination = destination[..len];
        if (!destinationChars.IsEmpty)
        {
            var hashSizeInBytes = hashAlgorithmTypeName.GetHMACHashSizeInBytes();
            if (destinationChars.Length >= hashSizeInBytes * 2)
            {
                if (lower)
                    Convert.TryToHexStringLower(destination.Span, destinationChars.Span, out var _);
                else
                    Convert.TryToHexString(destination.Span, destinationChars.Span, out var _);
            }
        }
    }
}