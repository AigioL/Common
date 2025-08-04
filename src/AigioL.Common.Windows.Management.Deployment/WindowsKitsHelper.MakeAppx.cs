using System.Diagnostics;
using Windows.Storage;

namespace Windows.Management.Deployment;

static partial class WindowsKitsHelper
{
    /// <summary>
    /// 应用包程序 (MakeAppx.exe) 从磁盘上的文件创建应用包，或将应用包中的文件提取到磁盘。
    /// <para>https://learn.microsoft.com/zh-cn/windows/win32/appxpkg/make-appx-package--makeappx-exe-</para>
    /// </summary>
    public static class MakeAppx
    {
        // https://learn.microsoft.com/zh-cn/windows/msix/package/create-app-package-with-makeappx-tool
        // 使用 MakeAppx.exe 创建 MSIX 包或捆绑包

        public const string makeappx_exe = "makeappx.exe";

        public static void Start(
            string msixPath, // Specifies the app package or bundle.
            string contentDirPath, // Specifies the input, output, or content directory
            string hashAlgorithm = "SHA256",
            string? makeAppxPath = null)
        {
            IOPath.DeleteFile(msixPath);

            makeAppxPath ??= GetWindowsKitsFilePath(makeappx_exe);

            var psi = new ProcessStartInfo
            {
                FileName = makeAppxPath,
                UseShellExecute = false,
                Arguments =
$"""
pack /v /h {hashAlgorithm} /d "{contentDirPath}" /p "{msixPath}"
""",
            };
            StartAndWaitForExit(psi);
        }

        public static void StartBundle(
            string msixPath, // Specifies the app package or bundle.
            string contentDirPath, // Specifies the input, output, or content directory
            string version4,
            string hashAlgorithm = "SHA256",
            string? makeAppxPath = null)
        {
            IOPath.DeleteFile(msixPath);

            makeAppxPath ??= GetWindowsKitsFilePath(makeappx_exe);
            var psi = new ProcessStartInfo
            {
                FileName = makeAppxPath,
                UseShellExecute = false,
                Arguments =
$"""
bundle /v /bv {version4} /h {hashAlgorithm} /d "{contentDirPath}" /p "{msixPath}"
""",
            };
            StartAndWaitForExit(psi);
        }

        ///// <summary>
        ///// 生成位于根目录的 AppxManifest.xml
        ///// </summary>
        ///// <param name="rootPublicPath"></param>
        ///// <param name="version4"></param>
        ///// <param name="processorArchitecture"></param>
        //public static void GenerateAppxManifestXml(
        //    string rootPublicPath,
        //    string version4,
        //    Architecture processorArchitecture)
        //{
        //    // https://learn.microsoft.com/zh-cn/windows/msix/desktop/desktop-to-uwp-manual-conversion

        //    var xmlString = XmlAppxManifest(version4, processorArchitecture);
        //    var xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(xmlString);
        //    var xmlStringMini = xmlDoc.InnerXml;
        //    var xmlFilePath = Path.Combine(rootPublicPath, "AppxManifest.xml");
        //    File.WriteAllText(xmlFilePath, xmlStringMini);
        //}

        //[Obsolete]
        //public static void GenerateBundleManifestXml(
        //    string dirPath,
        //    string version4)
        //{
        //    const string fileName = "BundleManifest.xml";

        //    // https://learn.microsoft.com/zh-cn/uwp/schemas/bundlemanifestschema/bundle-manifest

        //    var xmlString = XmlBundleManifest(version4);
        //    var xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(xmlString);
        //    var xmlStringMini = xmlDoc.InnerXml;
        //    var xmlFilePath = Path.Combine(dirPath, fileName);
        //    File.WriteAllText(xmlFilePath, xmlStringMini);
        //}

        //        /// <summary>
        //        /// 生成打包布局
        //        /// </summary>
        //        static void GeneratePackagingLayoutXml(
        //            string rootPublicPath,
        //            string appxManifestXmlPath,
        //            Architecture processorArchitecture)
        //        {
        //            // 使用打包布局创建包
        //            // https://learn.microsoft.com/zh-cn/windows/msix/package/packaging-layout

        //            var xmlString =
        //"""
        //<PackagingLayout xmlns="http://schemas.microsoft.com/appx/makeappx/2017">
        //  <PackageFamily ID="MyGame" FlatBundle="true" ManifestPath="{0}" ResourceManager="false">
        //    <!-- {1} code package-->
        //    <Package ID="{1}" ProcessorArchitecture="{1}">
        //      <Files>
        //        <File DestinationPath="*" SourcePath="{2}"/>
        //      </Files>
        //    </Package>
        //  </PackageFamily>
        //</PackagingLayout>
        //"""u8;
        //            Stream stream = new MemoryStream();
        //            stream.WriteFormat(xmlString, new object?[]
        //            {
        //                appxManifestXmlPath,
        //                processorArchitecture.ToString().ToLowerInvariant(),
        //                rootPublicPath,
        //            });
        //        }
    }
}
