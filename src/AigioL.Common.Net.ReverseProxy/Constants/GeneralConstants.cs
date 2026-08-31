namespace AigioL.Common.Net.ReverseProxy.Constants;

/// <summary>
/// 通用常量静态类
/// </summary>
public static class GeneralConstants
{
    /// <summary>
    /// 通用分隔符
    /// </summary>
    public const char GeneralSeparator = ';';

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    public const int StackallocByteThreshold = 256;
    public const int StackallocCharThreshold = StackallocByteThreshold / 2;

    public const int HTTP_PORT = 80;

    public const int HTTPS_PORT = 443;

    public const int GitHubDesktopPort = 9418;

    public const int SshPort = 22;

    public const string IPV6_TESTDOMAIN = "ipv6.rmbgame.net";

    /// <summary>
    /// 同源脚本注入路径前缀，避免使用外域 Script Src 造成页面脚本 Public Path 污染
    /// </summary>
    public const string InjectScriptPathPrefix = "/aigiolcommonnetreverseproxy_inject/";

    public static ReadOnlySpan<byte> InjectScriptPathPrefixU8() => "/aigiolcommonnetreverseproxy_inject/"u8;

    public const string HttpHeaderCookie = "cookie";

    public const string HttpHeaderReferer = "referer";

    public const string HttpHeader_SteamTool = "-steamtool";

    public const string LocalDomain = "local.rmbgame.net";

    public static readonly string HttpHeaderServer = "AigioL.Common.Net.ReverseProxy";

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/Common/src/System/Net/IPAddressParserStatics.cs#L9
    /// </summary>
    public const int IPv6AddressBytes = 16;

    public const string KeyGlobalProxy = "GlobalProxy";

    public const char TemplateStringVarPrefix = '@';

    public const string TemplateStringVarDomain = "@domain";

    public const string TemplateStringVarUri = "@uri";

    public const string TemplateStringVarOrigin = "${origin}";

    /// <summary>
    /// 995 (0x3E3) 由于发生线程退出或应用程序请求，I/O 操作已中止
    /// </summary>
    internal const int ERROR_OPERATION_ABORTED = 995;
}
