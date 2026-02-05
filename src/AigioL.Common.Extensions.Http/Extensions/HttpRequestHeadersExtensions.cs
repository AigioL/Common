using AngleSharp.Io;
using System.Net.Http.Headers;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

public static partial class HttpRequestHeadersExtensions
{
    const string AppleWebKitVersion = "537.36";
    const string SafariVersion = AppleWebKitVersion;
    const string ChromeVersionMajor = "144";
    const string EdgeVersionMajor = ChromeVersionMajor;

    public const string UserAgent =
$"""
Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/{AppleWebKitVersion} (KHTML, like Gecko) Chrome/{ChromeVersionMajor}.0.0.0 Safari/{SafariVersion} Edg/{EdgeVersionMajor}.0.0.0
""";

    public const string SecChUaPlatform =
"""
"Windows"
""";

    public const string SecChUaMobile =
"""
?0
""";

    public const string SecChUa =
$"""
"Not(A:Brand";v="8", "Chromium";v="{ChromeVersionMajor}", "Microsoft Edge";v="{EdgeVersionMajor}"
""";

    public static void SetUserAgent(this HttpRequestHeaders h)
    {
        h.UserAgent.ParseAdd(UserAgent);
        h.TryAddWithoutValidation("sec-ch-ua-platform", SecChUaPlatform);
        h.TryAddWithoutValidation("sec-ch-ua", SecChUa);
        h.TryAddWithoutValidation("sec-ch-ua-mobile", SecChUaMobile);
    }

    /// <summary>
    /// Accept-Language: zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6
    /// </summary>
    /// <param name="h"></param>
    public static void SetAcceptLanguage(this HttpRequestHeaders h)
    {
        h.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
    }

    /// <summary>
    /// HTTP DNT（请勿追踪）请求标头用于表示用户的跟踪偏好。它使用户能够表明自己更倾向于保护隐私，而非接收个性化内容
    /// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Reference/Headers/DNT</para>
    /// </summary>
    /// <param name="h"></param>
    /// <param name="doNotTrack">
    /// <see langword="false"/> 用户倾向于允许目标网站进行跟踪，<see langword="true"/> 用户倾向于不被目标网站跟踪，<see langword="null"/> 用户未指定关于跟踪的偏好
    /// </param>
    public static void SetDNT(this HttpRequestHeaders h, bool? doNotTrack = true)
    {
        var value = (doNotTrack.HasValue ? (doNotTrack.Value ? "1" : "0") : "null");
        h.TryAddWithoutValidation("DNT", value);
    }
}
