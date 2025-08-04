using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace Windows.Management.Deployment;

static partial class WindowsKitsHelper
{
    /// <summary>
    /// SignTool.exe 是一个命令行工具，用于对文件进行数字签名，以及验证文件和时间戳文件中的签名。
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/tools/signtool-exe</para>
    /// </summary>
    public static class SignTool
    {
        public const string SignTool_exe = "SignTool.exe";

        public static void Start(
            string fileName,
            string hashAlgorithm = "SHA256",
            string? signCertFile = null, // filePath OR SHA1 hash
            SecureString? signCertPassword = null,
            string? timestampUrl = null,
            string? workingDirectory = null,
            string? signToolPath = null)
        {
            signToolPath ??= GetWindowsKitsFilePath(SignTool_exe);

            bool isSHA1 = false;
            if (!string.IsNullOrWhiteSpace(signCertFile) && signCertFile.Length == SHA1.HashSizeInBytes * 2)
            {
                if (signCertFile.All("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains))
                {
                    isSHA1 = true;
                }
            }

            char[]? signCertPasswordBuffer = null;
            if (signCertPassword != null)
            {
                signCertPasswordBuffer = ArrayPool<char>.Shared.Rent(signCertPassword.Length);
            }

            try
            {
                ReadOnlySpan<char> signCertPasswordSpan = default;
                if (signCertPassword != null && signCertPasswordBuffer != null)
                {
                    nint signCertPasswordPtr = default;
                    try
                    {
                        signCertPasswordPtr = Marshal.SecureStringToGlobalAllocUnicode(signCertPassword);
                        for (int i = 0; i < signCertPassword.Length; i++)
                        {
                            signCertPasswordBuffer[i] = (char)Marshal.ReadInt16(signCertPasswordPtr, i * 2);
                        }
                        signCertPasswordSpan = signCertPasswordBuffer.AsSpan(0, signCertPassword.Length);
                    }
                    finally
                    {
                        Marshal.ZeroFreeGlobalAllocUnicode(signCertPasswordPtr);
                    }
                }
                ProcessStartInfo psi = new()
                {
                    FileName = GetWindowsKitsFilePath(SignTool_exe),
                    UseShellExecute = false,
                    Arguments =
$"""
sign /fd {hashAlgorithm} {(isSHA1 ? $"/sha1 {signCertFile} " :
$"""
/f "{signCertFile}" /p "{signCertPasswordSpan}"
""")}{(!string.IsNullOrWhiteSpace(timestampUrl) ? $"/tr {timestampUrl}" : null)} /td {hashAlgorithm} {fileName}
""",
                };
                if (!string.IsNullOrWhiteSpace(workingDirectory))
                {
                    psi.WorkingDirectory = workingDirectory;
                }
                StartAndWaitForExit(psi);
            }
            finally
            {
                if (signCertPasswordBuffer != null)
                {
                    ArrayPool<char>.Shared.Return(signCertPasswordBuffer);
                }
                try
                {
                    signCertPassword?.Dispose();
                }
                catch
                {
                }
            }
        }
    }
}