using System.Buffers;
#if USE_IO_HASHING
using System.IO.Hashing;
#endif

namespace System.Security.Cryptography;

public static class Hashs
{
#if USE_IO_HASHING
    static void HashData<TState, TNonCryptographicHashAlgorithm>(Stream s, TState state, SpanAction<byte, TState> action) where TNonCryptographicHashAlgorithm : NonCryptographicHashAlgorithm, new()
    {
        TNonCryptographicHashAlgorithm hashAlgorithm = new();
        Span<byte> b = stackalloc byte[hashAlgorithm.HashLengthInBytes];
        hashAlgorithm.Append(s);
        hashAlgorithm.TryGetCurrentHash(b, out var bytesWritten);
        if (bytesWritten != b.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesWritten), bytesWritten, null);
        }
        action(b, state);
    }
#endif

    public static void HashData<TState>(HashAlgorithmTypeName name, Stream s, TState state, SpanAction<byte, TState> action)
    {
        switch (name)
        {
            case HashAlgorithmTypeName.MD5:
                {
                    Span<byte> b = stackalloc byte[MD5.HashSizeInBytes];
                    MD5.HashData(s, b);
                    action(b, state);
                    break;
                }
            case HashAlgorithmTypeName.SHA1:
                {
                    Span<byte> b = stackalloc byte[SHA1.HashSizeInBytes];
                    SHA1.HashData(s, b);
                    action(b, state);
                    break;
                }
            case HashAlgorithmTypeName.SHA256:
                {
                    Span<byte> b = stackalloc byte[SHA256.HashSizeInBytes];
                    SHA256.HashData(s, b);
                    action(b, state);
                    break;
                }
            case HashAlgorithmTypeName.SHA384:
                {
                    Span<byte> b = stackalloc byte[SHA384.HashSizeInBytes];
                    SHA384.HashData(s, b);
                    action(b, state);
                    break;
                }
            case HashAlgorithmTypeName.SHA512:
                {
                    Span<byte> b = stackalloc byte[SHA512.HashSizeInBytes];
                    SHA512.HashData(s, b);
                    action(b, state);
                    break;
                }
#pragma warning disable CA1416 // 验证平台兼容性
            case HashAlgorithmTypeName.SHA3_256:
                {
                    Span<byte> b = stackalloc byte[SHA3_256.HashSizeInBytes];
                    SHA3_256.HashData(s, b);
                    action(b, state);
                    break;
                }
            case HashAlgorithmTypeName.SHA3_384:
                {
                    Span<byte> b = stackalloc byte[SHA3_384.HashSizeInBytes];
                    SHA3_384.HashData(s, b);
                    action(b, state);
                    break;
                }
            case HashAlgorithmTypeName.SHA3_512:
                {
                    Span<byte> b = stackalloc byte[SHA3_512.HashSizeInBytes];
                    SHA3_512.HashData(s, b);
                    action(b, state);
                    break;
                }
#pragma warning restore CA1416 // 验证平台兼容性
#if USE_IO_HASHING
            case HashAlgorithmTypeName.Crc32:
                {
                    HashData<TState, Crc32>(s, state, action);
                    break;
                }
            case HashAlgorithmTypeName.Crc64:
                {
                    HashData<TState, Crc64>(s, state, action);
                    break;
                }
#endif
            default:
                throw new ArgumentOutOfRangeException(nameof(name), name, null);
        }
    }

    public static void HashDataToHexString<TState>(HashAlgorithmTypeName name, Stream s, TState state, SpanAction<char, TState> action, bool isLower = false)
    {
        static void SpanAction(Span<byte> b, (bool isLower, TState state, SpanAction<char, TState> action) tuple)
        {
            Span<char> hex = stackalloc char[b.Length * 2];
            int charsWritten;
            if (tuple.isLower)
            {
                Convert.TryToHexStringLower(b, hex, out charsWritten);
            }
            else
            {
                Convert.TryToHexString(b, hex, out charsWritten);
            }
            if (charsWritten != hex.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(charsWritten), charsWritten, null);
            }
            tuple.action(hex, tuple.state);
        }

        HashData<(bool, TState, SpanAction<char, TState>)>(name, s, (isLower, state, action), SpanAction);
    }

    public static string HashDataToHexString(HashAlgorithmTypeName name, Stream s, bool isLower = false)
    {
        _StringState result = new();
        HashDataToHexString(name, s, result, static (span, state) =>
        {
            state.Result = new string(span);
        }, isLower);
        ArgumentNullException.ThrowIfNull(result.Result);
        return result.Result;
    }
}

#pragma warning disable IDE1006 // 命名样式
file sealed class _StringState
#pragma warning restore IDE1006 // 命名样式
{
    public string? Result { get; set; }
}