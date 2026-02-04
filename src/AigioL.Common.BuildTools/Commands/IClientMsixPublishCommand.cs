#if WINDOWS
using AigioL.Common.BuildTools.Commands.Abstractions;
using System.CommandLine;
using System.Reflection;
using System.Runtime.InteropServices;
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

        if (string.IsNullOrWhiteSpace(progPath) || !Version.TryParse(version4, out var version4Obj))
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
        WindowsKitsHelper.SignTool.Start($"\"{msixFilePath}\"", signCertFile: signCertFile);

        var msixBundleFilePath = $"{progPath}.msixbundle";
        WindowsKitsHelper.MakeAppx.StartBundle(msixBundleFilePath, msixDir, version4);
        await Task.Delay(TimeSpan.FromSeconds(1.15d));

        // 签名 msix 包
        // msix 签名证书名必须与包名一致
        WindowsKitsHelper.SignTool.Start($"\"{msixBundleFilePath}\"", signCertFile: signCertFile);

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

            var xmlString =
$"""
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<Package IgnorableNamespaces="uap rescap desktop desktop2 build" xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10" xmlns:desktop2="http://schemas.microsoft.com/appx/manifest/desktop/windows10/2" xmlns:build="http://schemas.microsoft.com/developer/appx/2015/build">
  <Identity Name="{IdentityName}" Publisher="{Publisher}" 
Version="{version4}" ProcessorArchitecture="{processorArchitecture.ToString().ToLowerInvariant()}"/>
  <Properties>
    <DisplayName>{DisplayName}</DisplayName>
    <PublisherDisplayName>{PublisherDisplayName}</PublisherDisplayName>
    <Logo>images\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0"/>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0"/>
  </Dependencies>
  <Resources>
    <Resource Language="zh-CN"/>
    <Resource uap:Scale="200"/>
  </Resources>
  <Applications>
    <Application Id="App" Executable="{Executable}" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="{DisplayName}" Description="{Description}" BackgroundColor="transparent" Square150x150Logo="images\Square150x150Logo.png" Square44x44Logo="images\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="images\Wide310x150Logo.png" Square71x71Logo="images\SmallTile.png" Square310x310Logo="images\LargeTile.png" ShortName="{DisplayName}">
          <uap:ShowNameOnTiles>
            <uap:ShowOn Tile="square150x150Logo"/>
            <uap:ShowOn Tile="wide310x150Logo"/>
            <uap:ShowOn Tile="square310x310Logo"/>
          </uap:ShowNameOnTiles>
        </uap:DefaultTile>
        <uap:SplashScreen Image="images\SplashScreen.png"/>
        <uap:InitialRotationPreference>
          <uap:Rotation Preference="landscape"/>
        </uap:InitialRotationPreference>
        <uap:LockScreen BadgeLogo="images\BadgeLogo.png" Notification="badgeAndTileText"/>
      </uap:VisualElements>
      <Extensions>
        <desktop:Extension Category="windows.fullTrustProcess" Executable="{Executable}"/>
        <desktop:Extension Category="windows.startupTask" Executable="{Executable}" EntryPoint="Windows.FullTrustApplication">
          <desktop:StartupTask TaskId="BootAutoStartTask" Enabled="true" DisplayName="{DisplayName} System Boot Run"/>
        </desktop:Extension>
      </Extensions>
    </Application>
  </Applications>
{PackageExtensions}
  <Capabilities>
    <Capability Name="internetClient"/>
    <rescap:Capability Name="runFullTrust"/>
    <rescap:Capability Name="allowElevation"/>
  </Capabilities>
</Package>
""";
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var xmlStringMini = xmlDoc.InnerXml;
            var xmlFilePath = Path.Combine(rootPublicPath, "AppxManifest.xml");
            File.WriteAllText(xmlFilePath, xmlStringMini);
        }
    }

    internal static class MakePri
    {
        public static void Start(string rootPublicPath)
        {
            var xmlPriConfig = @$"{rootPublicPath}\priconfig.xml";
            var xmlAppXManifestPath = @$"{rootPublicPath}\AppXManifest.xml";
            var projectRoot = $@"{ProjPath}\res\windows\makepri";

            WindowsKitsHelper.MakePri.Start(xmlPriConfig,
                xmlAppXManifestPath,
                projectRoot,
                rootPublicPath);
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
            var fields = typeof(IClientMsixPublishCommand).GetFields(BindingFlags.Instance | BindingFlags.Static);
            ConstReflectionHelper.fields = fields.ToDictionary(x => x.Name, x => x.GetValue(null)?.ToString())!;
        }
        return fields[name];
    }

#pragma warning disable IDE1006 // 命名样式
    internal static string IdentityName => GetConstString(nameof(IdentityName));
    internal static string Publisher => GetConstString(nameof(Publisher));
    internal static string DisplayName => GetConstString(nameof(DisplayName));
    internal static string PublisherDisplayName => GetConstString(nameof(PublisherDisplayName));
    internal static string Executable => GetConstString(nameof(Executable));
    internal static string Description => GetConstString(nameof(Description));
    internal static string PackageExtensions => GetConstString(nameof(PackageExtensions));
    internal static string pfxFilePath_MSStore_CodeSigning => GetConstString(nameof(pfxFilePath_MSStore_CodeSigning));
#pragma warning restore IDE1006 // 命名样式
}
#endif