using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Windows.Management.Deployment;

/// <summary>
/// Windows SDK 助手类
/// </summary>
public static partial class WindowsKitsHelper
{
    public enum WinVersion : ushort
    {
        W10_20H1 = 19041,
        W11_21H2 = 22000,
        W11_22H2 = 22621,
    }

    static string GetWindowsKitsFilePath(string fileName)
    {
        var query = from m in Enum.GetValues<WinVersion>().OrderByDescending(x => (ushort)x)
                    let p = GetWindowsKitsFilePath(m, fileName)
                    let exists = File.Exists(p)
                    where exists
                    select p;
        var path = query.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(path))
            throw new FileNotFoundException(null, fileName);
        return path;
    }

    static string GetWindowsKitsFilePath(WinVersion version, string fileName)
    {
        var osArchLower = RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant();
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        path = Path.Combine(path, "Windows Kits", "10", "bin", $"10.0.{(ushort)version}.0", osArchLower, fileName);
        return path;
    }

    ///// <summary>
    ///// 文件是否经过了数字签名
    ///// </summary>
    ///// <param name="filePath"></param>
    ///// <returns></returns>
    //public static bool IsDigitalSigned(string filePath)
    //{
    //    //var runspaceConfiguration = RunspaceConfiguration.Create();
    //    //using var runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
    //    //runspace.Open();
    //    //using var pipeline = runspace.CreatePipeline();
    //    //pipeline.Commands.AddScript("Get-AuthenticodeSignature \"" + filePath + "\"");
    //    //var results = pipeline.Invoke();
    //    //runspace.Close();
    //    //var signature = results[0].BaseObject as Signature;
    //    // https://github.com/PowerShell/PowerShell/blob/v7.2.4/src/Microsoft.PowerShell.Security/security/SignatureCommands.cs#L269-L282
    // Microsoft.PowerShell.Security https://learn.microsoft.com/en-us/dotnet/api/microsoft.powershell.commands.getauthenticodesignaturecommand
    //    var signature = SignatureHelper.GetSignature(filePath, null);
    //    var r = signature != null &&
    //        signature.SignerCertificate != null &&
    //        (signature.Status != SignatureStatus.NotSigned);
    //    return r;
    //}
}

unsafe partial class WindowsKitsHelper
{
    static delegate* managed<ProcessStartInfo, bool, void> StartAndWaitForExitFuncPtr;

    public static void SetStartAndWaitForExitFuncPtr(delegate* managed<ProcessStartInfo, bool, void> funcPtr)
    {
        StartAndWaitForExitFuncPtr = funcPtr;
    }

    /// <summary>
    /// 启动进程并等待退出，如果 ExitCode 不为 0 则引发 <see cref="ArgumentOutOfRangeException"/>
    /// </summary>
    /// <param name="psi"></param>
    /// <param name="ignoreExitCode">是否忽略 ExitCode 避免引发异常</param>
    static void StartAndWaitForExit(ProcessStartInfo psi, bool ignoreExitCode = false)
    {
        if (StartAndWaitForExitFuncPtr != default)
        {
            StartAndWaitForExitFuncPtr(psi, ignoreExitCode);
        }
        else
        {
            var process = Process.Start(psi);
            ArgumentNullException.ThrowIfNull(process);
            process.WaitForExit();
            if (ignoreExitCode)
                return;
            var exitCode = process.ExitCode;
            if (exitCode != default)
            {
                throw new ArgumentOutOfRangeException(nameof(exitCode),
                    $"Process {psi.FileName} exited with code {exitCode}.");
            }
        }
    }
}