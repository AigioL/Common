using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Storage;

namespace Windows.Management.Deployment;

static partial class WindowsKitsHelper
{
    /// <summary>
    /// MakePri.exe 是可用于创建和转储 PRI 文件的命令行工具。
    /// <para>https://learn.microsoft.com/zh-cn/windows/uwp/app-resources/compile-resources-manually-with-makepri</para>
    /// </summary>
    public static partial class MakePri
    {
        public const string makepri_exe = "makepri.exe";

        public static void Start(
            string xmlPriConfig, // {0}\priconfig.xml
            string xmlAppXManifestPath, // {0}\AppXManifest.xml
            string projectRoot,
            string? workingDirectory = null,
            string defaultCultureName = "zh-CN",
            string platform = "10.0.0",
            string? makePriPath = null)
        {
            IOPath.DeleteFile(xmlPriConfig);

            makePriPath ??= GetWindowsKitsFilePath(makepri_exe);
            var psi = new ProcessStartInfo
            {
                FileName = makePriPath,
                UseShellExecute = false,
                Arguments =
                // https://learn.microsoft.com/en-us/windows/uwp/app-resources/makepri-exe-command-options#createconfig-command
$"""
createconfig /cf "{xmlPriConfig}" /dq {defaultCultureName} /o /pv {platform}
""",
            };
            if (workingDirectory != null)
            {
                psi.WorkingDirectory = workingDirectory;
            }
            StartAndWaitForExit(psi);

            var prixml = File.ReadAllText(xmlPriConfig);
            prixml = prixml.Replace("<packaging>\r\n\t\t<autoResourcePackage qualifier=\"Language\"/>\r\n\t\t<autoResourcePackage qualifier=\"Scale\"/>\r\n\t\t<autoResourcePackage qualifier=\"DXFeatureLevel\"/>\r\n\t</packaging>", "");
            File.WriteAllText(xmlPriConfig, prixml);

            //var prPath = $@"{ProjPath}\res\windows\makepri";
            //CopyDirectory(prPath, rootPublicPath, true);
            psi = new ProcessStartInfo
            {
                FileName = makePriPath,
                UseShellExecute = false,
                Arguments =
                // https://learn.microsoft.com/en-us/windows/uwp/app-resources/makepri-exe-command-options#new-command
$"""
new /cf "{xmlAppXManifestPath}" /pr "{projectRoot}" /mn "{xmlAppXManifestPath}"
""",
            };
            if (workingDirectory != null)
            {
                psi.WorkingDirectory = workingDirectory;
            }
            StartAndWaitForExit(psi);

            IOPath.DeleteFile(xmlPriConfig);
        }
    }
}