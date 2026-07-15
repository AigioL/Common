using MemoryPack;
using Microsoft.IO;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Security.Cryptography;

public static partial class AesExtensions
{
    /// <summary>
    /// 使用可回收内存流管理器序列化（MemoryPack）对象并进行 AES 加密，返回密文流，由调用方释放
    /// </summary>
    public static async Task<RecyclableMemoryStream> SerializeEncryptAsync<T>(
        this Aes aes,
        RecyclableMemoryStreamManager m,
        T obj,
        CancellationToken cancellationToken = default)
    {
        using var serializeStream = m.GetStream();
        await MemoryPackSerializer.SerializeAsync(serializeStream, obj, cancellationToken: cancellationToken);
        serializeStream.Position = 0;

        var encryptStream = m.GetStream();
        using CryptoStream cryptoStream = new(encryptStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
        await serializeStream.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);
        encryptStream.Position = 0;
        return encryptStream;
    }

    /// <summary>
    /// 使用可回收内存流管理器序列化（MemoryPack）对象并进行 AES 加密，返回 Base64Url 编码的密文字符串
    /// </summary>
    public static async Task<string> SerializeEncryptReStringAsync<T>(
        this Aes aes,
        RecyclableMemoryStreamManager m,
        T obj,
        CancellationToken cancellationToken = default)
    {
        using var encryptStream = await aes.SerializeEncryptAsync(m, obj, cancellationToken);

        var span = encryptStream.GetBuffer().AsSpan()[..unchecked((int)encryptStream.Length)];
        var r = Base64Url.EncodeToString(span);
        return r;
    }

    /// <summary>
    /// 使用可回收内存流管理器序列化（Json+源生成）对象并进行 AES 加密，返回 Base64Url 编码的密文字符串
    /// </summary>
    public static async Task<string> SerializeEncryptToJsonAsync<T>(
        this Aes aes,
        RecyclableMemoryStreamManager m,
        T obj,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        using var serializeStream = m.GetStream();
        await JsonSerializer.SerializeAsync(serializeStream, obj, jsonTypeInfo, cancellationToken: cancellationToken);
        serializeStream.Position = 0;

        using var encryptStream = m.GetStream();
        using CryptoStream cryptoStream = new(encryptStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
        await serializeStream.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);
        encryptStream.Position = 0;

        var span = encryptStream.GetBuffer().AsSpan()[..unchecked((int)encryptStream.Length)];
        var r = Base64Url.EncodeToString(span);
        return r;
    }
}
