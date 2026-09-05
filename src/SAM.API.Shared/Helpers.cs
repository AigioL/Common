using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace SAM.API;

internal static partial class Helpers
{
    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    public const int StackallocByteThreshold = 256;

    public const int MemoryBufferSize = 1024 * 32;

    internal static ReadOnlySpan<byte> MemoryToSpan(ReadOnlySpan<byte> buffer)
    {
        var index = buffer.IndexOf((byte)0);
        if (index == -1)
        {
            return buffer;
        }
        return buffer[..index];
    }

    internal static readonly Encoding Ansi = GetAnsi_();

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("kernel32.dll", EntryPoint = "GetACP", ExactSpelling = true)]
#pragma warning disable SYSLIB1054 // 使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码
    private static extern int GetACP();
#pragma warning restore SYSLIB1054 // 使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码

    private static Encoding GetAnsi_()
    {
#if !WINDOWS
        if (OperatingSystem.IsWindows())
#endif
        {
            int codePage = GetACP();
            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(codePage);
            return encoding ?? Encoding.UTF8;
        }
#if !WINDOWS
        return Encoding.UTF8;
#endif
    }
}