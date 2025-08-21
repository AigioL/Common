using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.ApiGateway;

/// <summary>
/// 服务器证书（HTTPS/TLS 证书）的 <see cref="MemoryPack"/> 传输结构体
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.SerializeLayout.Explicit)]
public readonly partial struct ServerCertificatePackable
{
    /// <summary>
    /// 证书的数据
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public required byte[] RawData { get; init; }

    /// <summary>
    /// 证书的密码
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public string? Password { get; init; }

    /// <summary>
    /// 证书的类型
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public required ServerCertificateRawDataType Type { get; init; }

    /// <summary>
    /// PEM 格式证书的密钥数据
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public byte[]? KeyPem { get; init; }

    public X509Certificate2? GetX509Certificate2()
    {
        switch (Type)
        {
            case ServerCertificateRawDataType.PFX:
                {
#pragma warning disable SYSLIB0057 // 类型或成员已过时
                    return new X509Certificate2(RawData, Password);
#pragma warning restore SYSLIB0057 // 类型或成员已过时
                }
            case ServerCertificateRawDataType.PEM:
                {
                    int charCount;
                    X509Certificate2 x509Certificate2;

                    charCount = Encoding.UTF8.GetCharCount(RawData);
                    var certPem = ArrayPool<char>.Shared.Rent(charCount);

                    ArgumentNullException.ThrowIfNull(KeyPem);
                    charCount = Encoding.UTF8.GetCharCount(KeyPem);
                    var keyPem = ArrayPool<char>.Shared.Rent(charCount);
                    try
                    {
                        Encoding.UTF8.GetChars(RawData, certPem);
                        Encoding.UTF8.GetChars(KeyPem, keyPem);
                        x509Certificate2 = Password == null ? X509Certificate2.CreateFromPem(certPem, keyPem) : X509Certificate2.CreateFromEncryptedPem(certPem, keyPem, Password);
                        var bytes = x509Certificate2.Export(X509ContentType.Pfx);
#pragma warning disable SYSLIB0057 // 类型或成员已过时
                        x509Certificate2 = new X509Certificate2(bytes);
#pragma warning restore SYSLIB0057 // 类型或成员已过时
                    }
                    finally
                    {
                        ArrayPool<char>.Shared.Return(certPem);
                        ArrayPool<char>.Shared.Return(keyPem);
                    }
                    return x509Certificate2;
                }
            default:
                return null;
        }
    }

    public byte[] ToByteArray() => global::MemoryPack.MemoryPackSerializer.Serialize(this);

    public static ServerCertificatePackable Parse(byte[] bytes)
        => global::MemoryPack.MemoryPackSerializer.Deserialize<ServerCertificatePackable>(bytes);

    public static implicit operator byte[](ServerCertificatePackable value) => value.ToByteArray();

    public static implicit operator ServerCertificatePackable(byte[] value) => Parse(value);

    public static implicit operator X509Certificate2?(ServerCertificatePackable value) => value.GetX509Certificate2();
}
