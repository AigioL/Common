using System.Buffers;

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
}
