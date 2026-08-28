#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

static partial class HttpRequestOptionsExtensions
{
    const string TAG = "HttpReqOptExt";

    static readonly HttpRequestOptionsKey<bool> KeyIsHttps = new($"{TAG}_IsHttps");
    static readonly HttpRequestOptionsKey<TlsSniPattern> KeyTlsSniPattern = new($"{TAG}_TlsSniPattern");

    public static void SetIsHttps(this HttpRequestOptions o, bool v) => o.Set(KeyIsHttps, v);

    public static bool GetIsHttps(this HttpRequestOptions o)
    {
        if (o.TryGetValue(KeyIsHttps, out var v))
        {
            return v;
        }
        throw new InvalidOperationException($"HttpRequestOptions 中未设置 {nameof(KeyIsHttps)}");
    }

    public static void SetTlsSniPattern(this HttpRequestOptions o, TlsSniPattern v) => o.Set(KeyTlsSniPattern, v);

    public static TlsSniPattern GetTlsSniPattern(this HttpRequestOptions o)
    {
        if (o.TryGetValue(KeyTlsSniPattern, out var v))
        {
            return v;
        }
        throw new InvalidOperationException($"HttpRequestOptions 中未设置 {nameof(KeyTlsSniPattern)}");
    }
}