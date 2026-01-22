using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using System.Buffers;
using System.Buffers.Text;
using System.Security.Cryptography;

namespace AigioL.Common.AspNetCore.AppCenter.Services;

sealed partial class DataProtectionService<TAppSettings>(IOptions<TAppSettings> options) :
    DataProtectionService
    where TAppSettings : AppSettingsBase
{
    readonly TAppSettings settings = options.Value;

    protected override string DataProtectionKey
    {
        get
        {
            ArgumentNullException.ThrowIfNull(settings.DataProtectionKey);
            return settings.DataProtectionKey;
        }
    }
}

abstract partial class DataProtectionService : IDataProtectionService
{
    protected Lazy<(RSA rsa, byte[] keyHash)>? lazyRSA;

    protected abstract string DataProtectionKey { get; }

    protected DataProtectionService()
    {
        lazyRSA = new(CreateRSA, true);
    }

    (RSA rsa, byte[] keyHash) CreateRSA()
    {
        var bytes = Base64Url.DecodeFromChars(DataProtectionKey);

        var para = RSAUtils.ReadParameters(bytes);
        var rsa = RSA.Create(para);

        // 使用公钥的 MD5 作为密钥标识
        var keyHash = MD5.HashData(rsa.ExportRSAPublicKey()); // 这里固定 MD5 不能改
        return (rsa, keyHash);
    }

    (RSA rsa, byte[] keyHash) GetRSA()
    {
        var rsa = lazyRSA ?? throw new ObjectDisposedException(nameof(lazyRSA));
        return rsa.Value;
    }

    // 格式：
    // 1 字节 版本号（预留，目前固定为 0）
    // 16 字节 公钥哈希值（MD5）RSA 公钥的 MD5 哈希值，用于判断加密解密时使用的密钥是否一致
    // int32 分块密文长度 + 分块密文 数据块，重复直到结束

    public ValueTask<byte[]> ProtectAsync(byte[] plainBytes, CancellationToken cancellationToken = default)
    {
        var rsa = GetRSA();
        var r = Protect(rsa.rsa, rsa.keyHash, plainBytes, cancellationToken);
        return new(r);
    }

    public ValueTask<byte[]> UnprotectAsync(byte[] protectedBytes, CancellationToken cancellationToken = default)
    {
        var rsa = GetRSA();
        using var stream = m.GetStream();
        Unprotect(rsa.rsa, rsa.keyHash, protectedBytes, stream, cancellationToken);
        var r = stream.GetBuffer().AsSpan(0, (int)stream.Length).ToArray();
        return new(r);
    }
}

partial class DataProtectionService
{
    readonly RecyclableMemoryStreamManager m = new();

    static void Write(Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BitConverter.TryWriteBytes(bytes, value);
        stream.Write(bytes);
    }

    static int ReadInt32(Stream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        stream.ReadExactly(bytes);
        return BitConverter.ToInt32(bytes);
    }

    byte[] Protect(RSA rsa, byte[] keyHash, byte[] plainBytes, CancellationToken cancellationToken = default)
    {
        var max = GetMaxByteLength(rsa);

        using var stream = m.GetStream();
        stream.WriteByte(0); // 版本号，预留

        stream.Write(keyHash); // 写入密钥哈希值，以便解密时校验

        var span = plainBytes.AsSpan();
        while (span.Length > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (span.Length >= max)
            {
                var e = rsa.Encrypt(span[..max], Padding);
                Write(stream, e.Length);
                stream.Write(e);
                span = span[max..];
            }
            else
            {
                var e = rsa.Encrypt(plainBytes, Padding);
                Write(stream, e.Length);
                stream.Write(e);
                break;
            }
        }

        var r = stream.GetBuffer().AsSpan(0, (int)stream.Length).ToArray();
        return r;
    }

    void Unprotect(RSA rsa, byte[] keyHash, Span<byte> protectedBytes, Stream writeStream, CancellationToken cancellationToken = default)
    {
        //var version = protectedBytes[0]; // 目前只有版本号 0

        protectedBytes = protectedBytes[1..]; // 跳过版本号

        var max = GetMaxByteLength(rsa);

        var existingKeyHash = protectedBytes[..keyHash.Length];
        if (!existingKeyHash.SequenceEqual(keyHash))
        {
            throw new InvalidOperationException("数据保护解密时，传入的私钥与密文加密使用的公钥不同。");
        }

        protectedBytes = protectedBytes[keyHash.Length..];

        byte[]? array = null;
        Span<byte> utf8Bytes = max <= BufferWriterExtensions.StackallocByteThreshold ?
            stackalloc byte[BufferWriterExtensions.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(max));
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var len = BitConverter.ToInt32(protectedBytes);
                protectedBytes = protectedBytes[sizeof(int)..];

                var it = protectedBytes[..len];
                var m = rsa.Decrypt(it, Padding);

                writeStream.Write(m);
                protectedBytes = protectedBytes[len..];
                if (protectedBytes.Length == 0)
                {
                    break;
                }
            }
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }

    }

    protected virtual RSAEncryptionPadding Padding
    {
        get
        {
#if DEBUG
            if (OperatingSystem.IsWindows())
            {
                if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 25324))
                {
                    throw new NotSupportedException("当前操作系统不满足 Windows 11 内部版本 25324+，见 https://learn.microsoft.com/zh-cn/dotnet/standard/security/cross-platform-cryptography#hash-and-message-authentication-algorithms");
                }
            }
#endif

            return RSAEncryptionPadding.OaepSHA3_512;
        }
    }

    protected virtual int GetMaxByteLength(RSA rsa)
    {
        var k = rsa.KeySize / 8; // 密钥 4096 位 => k = 4096 / 8 = 512 字节
        var max = k - 2 * 64 - 2; // 若 OAEP 使用 SHA3-512（输出 512 位 = 64 字节），则 max = 512 - 2*64 - 2 = 382 字节
        return max;
    }
}

partial class DataProtectionService : IDisposable
{
    bool disposedValue;

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                if (lazyRSA != null && lazyRSA.IsValueCreated)
                {
                    lazyRSA.Value.rsa.Dispose();
                }
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            lazyRSA = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}