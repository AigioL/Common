using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AigioL.Common.UnitTest;

public sealed partial class EncodingTest : BaseUnitTest
{
    [Fact]
    public void AnsiTest()
    {
        var ansi = global::SAM.API.Helpers.Ansi;

        Assert.NotNull(ansi);
        Console.WriteLine($"Ansi: {ansi.EncodingName}");
    }

    [Fact]
    public void AcpTest()
    {
#if !WINDOWS
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
#endif
        var acp = GetACP();
        Console.WriteLine($"ACP: {acp}");

        var encoding = CodePagesEncodingProvider.Instance.GetEncoding(acp);
        Assert.NotNull(encoding);
        Console.WriteLine($"Ansi: {encoding.EncodingName}");
    }

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("kernel32.dll", EntryPoint = "GetACP", ExactSpelling = true)]
#pragma warning disable SYSLIB1054 // 使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码
    private static extern int GetACP();
#pragma warning restore SYSLIB1054 // 使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码
}
