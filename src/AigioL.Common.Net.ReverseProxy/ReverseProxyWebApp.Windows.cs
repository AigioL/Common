#if WINDOWS
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AigioL.Common.Net.ReverseProxy;

partial class ReverseProxyWebApp
{
    /// <summary>
    /// https://learn.microsoft.com/zh-cn/dotnet/core/extensions/globalization-icu#determine-if-your-app-is-using-icu
    /// </summary>
    /// <returns></returns>
    static bool ICUMode()
    {
        SortVersion sortVersion = CultureInfo.InvariantCulture.CompareInfo.Version;
        byte[] bytes = sortVersion.SortId.ToByteArray();
        int version = bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
        return version != 0 && version == sortVersion.FullVersion;
    }

    /// <summary>
    /// 在 ASP.NET Core 内部的 MiddlewareConfigurationManager 类中调用了 HostString， 如果在早期 Windows 上使用 ICU 模式时，则可能会抛出异常，当抛出异常时，需要回退到 NLS，这需要更改 .NET 运行时配置以及重启 .NET 运行时
    /// </summary>
    static bool IcuTest(IEnumerable<string> incoming)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362) && ICUMode())
        {
            // https://learn.microsoft.com/zh-cn/dotnet/core/extensions/globalization-icu#icu-on-windows
            // 与 .NET 6 和 .NET 5 相比，.NET 7 及更高版本能够在早期 Windows 版本上加载 ICU
            // 通常问题出在 1703 ~ 1903 之间的 Windows 10 版本
            try
            {
                // https://github.com/dotnet/aspnetcore/blob/v11.0.0-preview.7.26381.103/src/Middleware/HostFiltering/src/MiddlewareConfigurationManager.cs#L64-L67
                foreach (var entry in incoming)
                {
                    // Punycode. Http.Sys requires you to register Unicode hosts, but the headers contain punycode.
                    var host = new HostString(entry).ToUriComponent();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }
}
#endif