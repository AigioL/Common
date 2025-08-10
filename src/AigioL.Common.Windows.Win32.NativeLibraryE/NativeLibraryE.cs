using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace System.Runtime.InteropServices;

/// <summary>
/// 使用 AES 加密本机库名与函数名的加载助手类
/// <para>使用加密函数名运行时解密避免被“安全软件”检测或拦截</para>
/// </summary>
public static class NativeLibraryE
{
    public static nint Load(ReadOnlySpan<byte> libraryPath, DllImportSearchPath? searchPath = DllImportSearchPath.System32)
    {
        var libraryPathStr = E31c63fd.Decrypt(libraryPath);
        var libPtr = NativeLibrary.Load(libraryPathStr,
            typeof(NativeLibraryE).Assembly,
            searchPath);
        return libPtr;
    }

    public static nint Load(ReadOnlySpan<byte> libraryPath, out string libraryPathStr, DllImportSearchPath? searchPath = DllImportSearchPath.System32)
    {
        libraryPathStr = E31c63fd.Decrypt(libraryPath);
        var libPtr = NativeLibrary.Load(libraryPathStr,
            typeof(NativeLibraryE).Assembly,
            searchPath);
        return libPtr;
    }

    public static nint GetExport(nint handle, ReadOnlySpan<byte> name)
    {
        var nameStr = E31c63fd.Decrypt(name);
        var methodPtr = NativeLibrary.GetExport(handle, nameStr);
        return methodPtr;
    }

    public static nint GetExport(nint handle, ReadOnlySpan<byte> name, out string nameStr)
    {
        nameStr = E31c63fd.Decrypt(name);
        var methodPtr = NativeLibrary.GetExport(handle, nameStr);
        return methodPtr;
    }
}

file static class E31c63fd
{
    /// <summary>
    /// 用于 AES 加密的密钥（Key）与向量（IV）
    /// </summary>
    const string GuidKey = "e7e04d83-6f5c-4d56-abf8-2debf0696cfc";

    /// <summary>
    /// 仅用于加密本机库名与函数名的 AES 加密，加密强度无关，仅用于避免明文字符串被“安全软件”检测或拦截
    /// </summary>
    static Aes A
    {
        get
        {
            if (field == null)
            {
                field = Aes.Create();
                field.KeySize = 128; // 128 bits 时密钥与向量长度均为 16 字节等于一个 GUID
                field.Key = Guid.Parse(GuidKey).ToByteArray();
            }
            return field;
        }
    }

    [DoesNotReturn]
    static void ThrowArgOutOfRange(object? actualValue, [CallerArgumentExpression(nameof(actualValue))] string? paramName = null)
        => throw new ArgumentOutOfRangeException(paramName, actualValue, null);

    internal static string Decrypt(ReadOnlySpan<byte> ciphertextU8)
    {
        var a = E31c63fd.A;

        // 传入的 UTF8 字节转换为 UTF16 字符组
        Span<char> chars = stackalloc char[256];
        var len = Encoding.UTF8.GetChars(ciphertextU8, chars);
        chars = chars[..len];
        // 创建字节组将 Hex 字符转换为字节
        Span<byte> bytes = stackalloc byte[len * 2];
        var operationStatus = Convert.FromHexString(chars, bytes, out var _, out var bytesWritten);
        if (operationStatus != OperationStatus.Done)
        {
            // 函数名与 dll 文件名都不会很长，在 stack 上申请内存也不会溢出
            // span 转换 hex 也不用分块循环，一次就够了，所以必定为 Done
            // 这里的 throw 应该不可能进来
            ThrowArgOutOfRange(operationStatus);
        }
        bytes = bytes[..bytesWritten];

        // AES 解密
        var plaintextU8 = a.DecryptCbc(bytes, a.Key, PaddingMode.PKCS7);
        var plaintext = Encoding.UTF8.GetString(plaintextU8);
        return plaintext;
    }

#if DEBUG
    internal static string Encrypt(string plaintext)
    {
        var plaintextU8 = Encoding.UTF8.GetBytes(plaintext);

        var a = E31c63fd.A;
        var ciphertext = a.EncryptCbc(plaintextU8, a.Key, PaddingMode.PKCS7);

        var ciphertextHex = Convert.ToHexString(ciphertext);
        return ciphertextHex;
    }
#endif
}