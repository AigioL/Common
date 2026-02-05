#if WINDOWS
using AigioL.Common.BuildTools.Commands.Abstractions;
using System.CommandLine;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Xml;
using Windows.Management.Deployment;
using static AigioL.Common.BuildTools.Commands.ConstReflectionHelper;

namespace AigioL.Common.BuildTools.Commands;

/// <summary>
/// 客户端程序 MSIX 发布命令
/// </summary>
public partial interface IClientMsixPublishCommand : ICommand
{
    /// <summary>
    /// 命令名
    /// </summary>
    const string CommandName = "cpub_msix";

    /// <inheritdoc cref="ICommand.GetCommand"/>
    static Command ICommand.GetCommand()
    {
        var progPath = new Option<string>("--progPath")
        {
            Description = "要打包的程序目录",
        };
        var version4 = new Option<string>("--ver4")
        {
            Description = "四位版本号",
        };
        var command = new Command(CommandName, "客户端发布命令")
        {
            progPath, version4,
        };
        command.SetAction(parseResult => Handler(
            parseResult.GetValue(progPath),
            parseResult.GetValue(version4)
        ));
        return command;
    }

    static async Task<int> Handler(string? progPath, string? version4)
    {
        const Architecture arch = Architecture.X64;
        bool? debug = null;
        string rid = $"win-{arch.ToString().ToLowerInvariant()}";
        var signCertFile = pfxFilePath_MSStore_CodeSigning;
        SecureString? signCertPassword = null;

        if (string.IsNullOrWhiteSpace(progPath))
        {
            return -1;
        }

        if (string.Equals("auto", version4, StringComparison.InvariantCultureIgnoreCase))
        {
            var exePath = Path.Combine(progPath, Executable);
            var fvi = FileVersionInfo.GetVersionInfo(exePath);
            version4 = fvi.FileVersion;
        }

        if (!Version.TryParse(version4, out var version4Obj))
        {
            return -1;
        }

        if (!Directory.Exists(progPath))
        {
            Console.WriteLine($"程序目录不存在：{progPath}");
            return -2;
        }

        version4 = version4Obj.ToVersion4(); // 确保版本号中有3个.为4个完整的版本号

        var bgOriginalColor = Console.BackgroundColor;
        var fgOriginalColor = Console.ForegroundColor;

        void ResetConsoleColor()
        {
            Console.BackgroundColor = bgOriginalColor;
            Console.ForegroundColor = fgOriginalColor;
        }
        void SetConsoleColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
        }

        SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
        Console.WriteLine("开始生成【MSIX 包】");
        ResetConsoleColor();

        // 生成清单文件
        MSIXHelper.MakeAppx.GenerateAppxManifestXml(progPath, version4, arch);

        // 打包资源 images
        MSIXHelper.MakePri.Start(progPath);

        var msixDir = $"{progPath}_MSIX";
        IOPath.CreateDirectory(msixDir);
        var msixFilePath = Path.Combine(msixDir, MSIXHelper.GetPublishFileName(debug, version4, rid, ".msix"));

        // 生成 msix 包
        WindowsKitsHelper.MakeAppx.Start(msixFilePath, progPath);
        await Task.Delay(TimeSpan.FromSeconds(1.15d));

        // 签名 msix 包
        // msix 签名证书名必须与包名一致
        WindowsKitsHelper.SignTool.Start($"\"{msixFilePath}\"", signCertFile: signCertFile, signCertPassword: signCertPassword);

        var msixBundleFilePath = Path.Combine(Path.GetDirectoryName(progPath)!, MSIXHelper.GetPublishFileName(debug, version4, rid, ".msixbundle"));
        WindowsKitsHelper.MakeAppx.StartBundle(msixBundleFilePath, msixDir, version4);
        await Task.Delay(TimeSpan.FromSeconds(1.15d));

        // 签名 msix 包
        // msix 签名证书名必须与包名一致
        WindowsKitsHelper.SignTool.Start($"\"{msixBundleFilePath}\"", signCertFile: signCertFile, signCertPassword: signCertPassword);

        var msixBundleFileInfo = new FileInfo(msixBundleFilePath);

        SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
        Console.Write("已生成【MSIX 包】，文件大小：");
        Console.Write(IOPath.GetDisplayFileSizeString(msixBundleFileInfo.Length));
        Console.Write("，路径：");
        Console.WriteLine(msixFilePath);
        ResetConsoleColor();

        return 0;
    }
}

file static class MSIXHelper
{
    static int GetInt32(int value) => value < 0 ? 0 : value;

    internal static string ToVersion4(this Version v)
        => $"{GetInt32(v.Major)}.{GetInt32(v.Minor)}.{GetInt32(v.Build)}.{GetInt32(v.Revision)}";

    internal static string releaseTimestamp = DateTimeOffset.Now.ToString("yyMMdd_HHmmssfffffff");

    internal static string GetPublishFileName(bool? debug, string version, string rid, string fileEx = "")
    {
        var value = $"{(debug.HasValue ? ($"[{(debug.Value ? "Debug" : "Release")}] ") : null)}{Path.GetFileNameWithoutExtension(Executable)}_v{version}_{rid.Replace('-', '_')}_{releaseTimestamp}{fileEx}";
        return value;
    }

    internal static class MakeAppx
    {
        /// <summary>
        /// 生成位于根目录的 AppxManifest.xml
        /// </summary>
        public static void GenerateAppxManifestXml(
            string rootPublicPath,
            string version4,
            Architecture processorArchitecture)
        {
            // https://learn.microsoft.com/zh-cn/windows/msix/desktop/desktop-to-uwp-manual-conversion

            // 处理 URI 激活
            // https://learn.microsoft.com/zh-cn/windows/uwp/launch-resume/handle-uri-activation

            var processorArch = processorArchitecture.ToString().ToLowerInvariant();
            var xmlString = AppxManifestXml;
            xmlString = xmlString.Replace("[version4]", version4);
            xmlString = xmlString.Replace("[processorArch]", processorArch);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var xmlStringMini = xmlDoc.InnerXml;
            var xmlFilePath = Path.Combine(rootPublicPath, "AppxManifest.xml");
            File.WriteAllText(xmlFilePath, xmlStringMini, new UTF8Encoding(true));
        }
    }

    internal static class MakePri
    {
        public static void Start(string rootPublicPath)
        {
            var xmlPriConfig = @$"{rootPublicPath}\priconfig.xml";
            var xmlAppXManifestPath = @$"{rootPublicPath}\AppXManifest.xml";
            var projectRoot = $@"{ProjPath}\res\windows\makepri";

            CopyDirectory(projectRoot, rootPublicPath, true);

            var priPath = @$"{rootPublicPath}\resources.pri";
            IOPath.DeleteFile(priPath);

            WindowsKitsHelper.MakePri.Start(
                xmlPriConfig,
                xmlAppXManifestPath,
                projectRoot,
                rootPublicPath);
        }
    }

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive) // https://learn.microsoft.com/zh-cn/dotnet/standard/io/how-to-copy-directories
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        IOPath.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}

file static class ConstReflectionHelper
{
    static Dictionary<string, string>? fields = null;

    internal static string GetConstString(string name)
    {
        if (fields == null)
        {
            var t = typeof(IClientMsixPublishCommand);
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                .Concat(t.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));
            ConstReflectionHelper.fields = fields
                .Where(x => x.FieldType == typeof(string))
                .ToDictionary(x => x.Name, x => x.GetValue(null)?.ToString())!;
        }
        return fields[name];
    }

#pragma warning disable IDE1006 // 命名样式
    internal static string AppxManifestXml => GetConstString(nameof(AppxManifestXml));
    internal static string Executable => GetConstString(nameof(Executable));
    internal static string pfxFilePath_MSStore_CodeSigning => GetConstString(nameof(pfxFilePath_MSStore_CodeSigning));
#pragma warning restore IDE1006 // 命名样式
}
#endif