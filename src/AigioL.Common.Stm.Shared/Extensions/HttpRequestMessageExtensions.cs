using static System.Net.Http.WebKitFormBoundary_MultipartFormDataContent;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

/// <summary>
/// <see cref="HttpRequestMessage"/> 的扩展函数
/// </summary>
public static partial class HttpRequestMessageExtensions
{
    public static string? GetWebKitFormBoundary(this HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(KeyWebKitFormBoundary, out var boundary))
        {
            return boundary;
        }
        return null;
    }

    public static void SetWebKitFormBoundary(this HttpRequestMessage request, string? boundary)
    {
        request.Options.Set(KeyWebKitFormBoundary, boundary);
    }

    /// <summary>
    /// 将键值对集合设置为请求的表单内容，Content-Type 为 multipart/form-data，boundary 为 WebKitFormBoundary 随机字符串
    /// </summary>
    public static void SetMultipartFormDataContentWithtWebKitFormBoundary(
       this HttpRequestMessage request,
       params IEnumerable<KeyValuePair<string?, string?>> nameValueCollection)
    {
        var boundary = request.GetWebKitFormBoundary();
        if (string.IsNullOrWhiteSpace(boundary))
        {
            boundary = GetWebKitFormBoundaryKeyValue();
            request.SetWebKitFormBoundary(boundary);
        }
        MultipartFormDataContent content = new(boundary);
        foreach (var it in nameValueCollection)
        {
            StringContent stringContent = new(it.Value ?? "");
            stringContent.Headers.ContentType = null; // 和 Steam 网页上调用保持一致，StringContent 构造函数会赋值 ContentType，这里清空
            stringContent.Headers.ContentDisposition = new("form-data")
            {
                Name = it.Key,
            };
            content.Add(stringContent);
        }
        request.Content = content;
    }

    /// <inheritdoc cref="SetMultipartFormDataContentWithtWebKitFormBoundary(HttpRequestMessage, IEnumerable{KeyValuePair{string?, string?}})"/>
    public static void SetMultipartFormDataContentWithtWebKitFormBoundary(
        this HttpRequestMessage request,
        Dictionary<string, string?> nameValueCollection)
    {
        IEnumerable<KeyValuePair<string?, string?>> nameValueCollection_ = nameValueCollection!;
        SetMultipartFormDataContentWithtWebKitFormBoundary(request, nameValueCollection_);
    }

    /// <inheritdoc cref="SetMultipartFormDataContentWithtWebKitFormBoundary(HttpRequestMessage, IEnumerable{KeyValuePair{string?, string?}})"/>
    public static void SetMultipartFormDataContentWithtWebKitFormBoundary(
        this HttpRequestMessage request,
        SortedDictionary<string, string?> nameValueCollection)
    {
        IEnumerable<KeyValuePair<string?, string?>> nameValueCollection_ = nameValueCollection!;
        SetMultipartFormDataContentWithtWebKitFormBoundary(request, nameValueCollection_);
    }
}

file static partial class WebKitFormBoundary_MultipartFormDataContent
{
    internal static readonly HttpRequestOptionsKey<string?> KeyWebKitFormBoundary = new("Opt----WebKitFormBoundary");

    const string WebKitFormBoundary = "----WebKitFormBoundary";

    const int WebKitFormBoundaryRandomLength = 16;

    internal static string GetWebKitFormBoundaryKeyValue()
    {
        const string randomChars = "qwertyuiopasdfghjklzxcvbnm1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
        Span<char> chars = stackalloc char[WebKitFormBoundaryRandomLength + WebKitFormBoundary.Length];
        WebKitFormBoundary.AsSpan().CopyTo(chars);

        var temp = chars[WebKitFormBoundary.Length..];
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i] = randomChars[Random.Shared.Next(randomChars.Length)];
        }

        return new string(chars);
    }
}