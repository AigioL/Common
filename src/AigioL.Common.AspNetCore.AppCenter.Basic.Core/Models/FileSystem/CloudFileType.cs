namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;

/// <summary>
/// 静态资源文件类型
/// </summary>
public enum CloudFileType
{
    // 不使用值为 0 的

    #region ImageFormat 1~255 值用于图片类型

    /// <summary>
    /// BMP 取自位图 Bitmap 的缩写，也称为 DIB（与设备无关的位图），是一种与显示器无关的位图数字图像文件格式。常见于 Microsoft Windows 和 OS/2 操作系统，Windows GDI API内部使用的DIB数据结构与 BMP 文件格式几乎相同。
    /// <para>https://en.wikipedia.org/wiki/BMP_file_format</para>
    /// </summary>
    BMP = 2,

    /// <summary>
    /// 图像互换格式（GIF，Graphics Interchange Format）是一种位图图形文件格式，以8位色（即256种颜色）重现真彩色的图像。它实际上是一种压缩文档，采用LZW压缩算法进行编码，有效地减少了图像文件在网络上传输的时间。它是目前万维网广泛应用的网络传输图像格式之一。
    /// <para>https://en.wikipedia.org/wiki/GIF</para>
    /// <para>https://baike.sogou.com/v2844.htm</para>
    /// </summary>
    GIF = 3,

    /// <summary>
    /// ICO 文件格式是 Microsoft Windows 中计算机图标的图像文件格式。 ICO 文件包含多个尺寸和颜色深度的一个或多个小图像，以便可以适当地缩放它们。 在Windows中，向用户，桌面，“开始”菜单或Windows资源管理器中显示图标的所有可执行文件都必须带有ICO格式的图标。
    /// <para>https://en.wikipedia.org/wiki/ICO_(file_format)</para>
    /// <para>https://baike.sogou.com/v64848120.htm</para>
    /// </summary>
    ICO = 4,

    /// <summary>
    /// JPG/JPEG
    /// 在计算机中，JPEG（发音为jay-peg, IPA：[ˈdʒeɪpɛg]）是一种针对照片视频而广泛使用的有损压缩标准方法。这个名称代表Joint Photographic Experts Group（联合图像专家小组）。此团队创立于1986年，1992年发布了JPEG的标准而在1994年获得了ISO 10918-1的认定。JPEG与视频音频压缩标准的MPEG（Moving Picture Experts Group）很容易混淆，但两者是不同的组织及标准。
    /// <para>https://en.wikipedia.org/wiki/JPEG</para>
    /// <para>https://baike.sogou.com/v241266.htm</para>
    /// </summary>
    JPEG = 5,

    /// <summary>
    /// 便携式网络图形（Portable Network Graphics，PNG）是一种无损压缩的位图图形格式，支持索引、灰度、RGB 三种颜色方案以及Alpha通道等特性。PNG的开发目标是改善并取代GIF作为适合网络传输的格式而不需专利许可，所以被广泛应用于互联网及其他方面上。
    /// <para>https://en.wikipedia.org/wiki/Portable_Network_Graphics</para>
    /// <para>https://baike.sogou.com/v44717.htm</para>
    /// </summary>
    PNG = 6,

    /// <summary>
    /// WebP（发音weppy），是一种同时提供了有损压缩与无损压缩（可逆压缩）的图片文件格式，派生自视频编码格式VP8，被认为是WebM多媒体格式的姊妹项目，是由Google在购买On2 Technologies后发展出来，以BSD授权条款发布。
    /// <para>https://zh.wikipedia.org/wiki/WebP</para>
    /// <para>https://baike.sogou.com/v10183483.htm</para>
    /// <para>https://developers.google.com/speed/webp/</para>
    /// <para>https://developers.google.cn/speed/webp/</para>
    /// </summary>
    WebP = 7,


    /// <summary>
    /// HEIF/HEIC
    /// 高效率图像文件格式（英语：High Efficiency Image File Format, HEIF；也称高效图像文件格式）是一个用于单张图像或图像序列的文件格式。它由运动图像专家组（MPEG）开发，并在MPEG-H Part 12（ISO/IEC 23008-12）中定义。
    /// <para>https://en.wikipedia.org/wiki/High_Efficiency_Image_File_Format</para>
    /// <para>https://baike.baidu.com/item/HEIC/10444257</para>
    /// </summary>
    HEIF = 8,

    /// <inheritdoc cref="HEIF"/>
    HEIFSequence = 9,

    /// <inheritdoc cref="HEIF"/>
    HEIC = 10,

    /// <inheritdoc cref="HEIF"/>
    HEICSequence = 11,

    /// <summary>
    /// Tagged Image File Format 标签图像文件格式
    /// </summary>
    TIFF = 12,

    #endregion

    #region 256~xxx 待定

    /// <summary>
    /// .exe
    /// </summary>
    WinExe = 256,

    /// <summary>
    /// .tar.gz
    /// </summary>
    TarGzip,

    /// <summary>
    /// .7z
    /// </summary>
    SevenZip,

    /// <summary>
    /// .tar.br
    /// </summary>
    TarBrotli,

    /// <summary>
    /// .tar.xz
    /// </summary>
    TarXz,

    /// <summary>
    /// .tar.zst
    /// </summary>
    TarZstd,

    /// <summary>
    /// .dmg
    /// </summary>
    DMG,

    /// <summary>
    /// .deb
    /// </summary>
    DEB,

    /// <summary>
    /// .rpm
    /// </summary>
    RPM,

    /// <summary>
    /// .apk
    /// </summary>
    APK,

    /// <summary>
    /// .zip
    /// </summary>
    ZIP,

    #endregion

    /// <summary>
    /// .json
    /// </summary>
    Json = 300,

    /// <summary>
    /// .dll
    /// </summary>
    Dll,

    /// <summary>
    /// .xml
    /// </summary>
    Xml,

    /// <summary>
    /// .so
    /// </summary>
    So,

    /// <summary>
    /// .dylib
    /// </summary>
    Dylib,

    /// <summary>
    /// 无
    /// </summary>
    None,

    /// <summary>
    /// .js
    /// </summary>
    Js,

    /// <summary>
    /// .xaml
    /// </summary>
    Xaml,

    /// <summary>
    /// .axaml
    /// </summary>
    AXaml,

    /// <summary>
    /// .cs
    /// </summary>
    CSharp,

    /// <summary>
    /// .msix
    /// </summary>
    Msix = 901,

    /// <summary>
    /// .msixupload
    /// </summary>
    MsixUpload,
}

/// <summary>
/// Enum 扩展 <see cref="CloudFileType"/>
/// </summary>
public static partial class CloudFileTypeEnumExtensions
{
    ///// <summary>
    ///// 将 <see cref="CloudFileType"/> 转换为 <see cref="ImageFormat"/>，值不为图片类型时将返回默认值
    ///// </summary>
    ///// <param name="type"></param>
    ///// <returns></returns>
    //public static ImageFormat GetImageFormat(this CloudFileType type)
    //{
    //    var value = Enum2.ConvertToInt32(type);
    //    if (value > 0 && value < byte.MaxValue)
    //    {
    //        var value2 = Convert.ToByte(value);
    //        return (ImageFormat)value2;
    //    }
    //    return default;
    //}

    ///// <summary>
    ///// 扩展名或者图片类型识别是否是支持的文件类型
    ///// </summary>
    ///// <param name="extension"></param>
    ///// <param name="imageFormat"></param>
    ///// <returns></returns>
    //public static CloudFileType? GetFileFormat(this string extension, ImageFormat? imageFormat)
    //{
    //    if (imageFormat.HasValue)
    //    {
    //        var value = Enum2.ConvertToInt32(imageFormat.Value);
    //        if (value > 0 && value < byte.MaxValue)
    //        {
    //            var value2 = Convert.ToByte(value);
    //            return (CloudFileType)value2;
    //        }
    //    }
    //    else
    //        return extension.ToLowerInvariant() switch
    //        {
    //            FileEx.EXE => (CloudFileType?)CloudFileType.WinExe,
    //            ".tar.gz" or FileEx.TAR_GZ => (CloudFileType?)CloudFileType.TarGzip,
    //            FileEx.TAR_XZ => (CloudFileType?)CloudFileType.TarXz,
    //            FileEx.TAR_ZST => (CloudFileType?)CloudFileType.TarZstd,
    //            FileEx.DMG => (CloudFileType?)CloudFileType.DMG,
    //            FileEx.DEB => (CloudFileType?)CloudFileType.DEB,
    //            FileEx.RPM => (CloudFileType?)CloudFileType.RPM,
    //            FileEx.APK => (CloudFileType?)CloudFileType.RPM,
    //            FileEx.ZIP => (CloudFileType?)CloudFileType.ZIP,
    //            _ => null,
    //        };
    //    return null;
    //}

    /// <summary>
    /// 根据文件扩展名识别文件类型
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static CloudFileType? GetFileFormat(this ReadOnlySpan<char> filePath)
    {
        if (filePath.IsWhiteSpace())
        {
            return null;
        }
        var split = filePath.Split('.');
        Range? last = default;
        Range? lastlast = default;
        while (split.MoveNext())
        {
            lastlast = last;
            last = split.Current;
        }
        if (last.HasValue)
        {
            var ext = filePath[last.Value];
            if (filePath.Length <= 10) // .msixupload
            {
                if (ext.Equals("bmp", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.BMP;
                }
                else if (ext.Equals("gif", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.GIF;
                }
                else if (ext.Equals("jpg", StringComparison.InvariantCultureIgnoreCase)
                    || ext.Equals("jpeg", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.JPEG;
                }
                else if (ext.Equals("ico", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.ICO;
                }
                else if (ext.Equals("png", StringComparison.InvariantCultureIgnoreCase)
                    || ext.Equals("apng", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.PNG;
                }
                else if (ext.Equals("webp", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.WebP;
                }
                else if (ext.Equals("heic", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.HEIC;
                }
                else if (ext.Equals("heif", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.HEIF;
                }
                else if (ext.Equals("tiff", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.TIFF;
                }
                else if (ext.Equals("exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.WinExe;
                }
                else if (ext.Equals("tgz", StringComparison.InvariantCultureIgnoreCase)
                    || (ext.Equals("gz", StringComparison.InvariantCultureIgnoreCase)
                    && lastlast.HasValue && filePath[lastlast.Value].Equals("tar", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return CloudFileType.TarGzip;
                }
                else if (ext.Equals("7z", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.SevenZip;
                }
                else if (ext.Equals("tbr", StringComparison.InvariantCultureIgnoreCase)
                    || (ext.Equals("br", StringComparison.InvariantCultureIgnoreCase)
                    && lastlast.HasValue && filePath[lastlast.Value].Equals("tar", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return CloudFileType.TarBrotli;
                }
                else if (ext.Equals("txz", StringComparison.InvariantCultureIgnoreCase)
                    || (ext.Equals("xz", StringComparison.InvariantCultureIgnoreCase)
                    && lastlast.HasValue && filePath[lastlast.Value].Equals("tar", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return CloudFileType.TarXz;
                }
                else if (ext.Equals("tzst", StringComparison.InvariantCultureIgnoreCase)
                    || (ext.Equals("zst", StringComparison.InvariantCultureIgnoreCase)
                    && lastlast.HasValue && filePath[lastlast.Value].Equals("tar", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return CloudFileType.TarZstd;
                }
                else if (ext.Equals("dmg", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.DMG;
                }
                else if (ext.Equals("deb", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.DEB;
                }
                else if (ext.Equals("rpm", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.RPM;
                }
                else if (ext.Equals("apk", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.APK;
                }
                else if (ext.Equals("zip", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.ZIP;
                }
                else if (ext.Equals("json", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.Json;
                }
                else if (ext.Equals("dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.Dll;
                }
                else if (ext.Equals("xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.Xml;
                }
                else if (ext.Equals("so", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.So;
                }
                else if (ext.Equals("dylib", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.Dylib;
                }
                else if (ext.Equals("js", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.Js;
                }
                else if (ext.Equals("xaml", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.Xaml;
                }
                else if (ext.Equals("axaml", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.AXaml;
                }
                else if (ext.Equals("cs", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.CSharp;
                }
                else if (ext.Equals("msix", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.Msix;
                }
                else if (ext.Equals("msixupload", StringComparison.InvariantCultureIgnoreCase))
                {
                    return CloudFileType.MsixUpload;
                }
            }
        }
        return null;
    }
}