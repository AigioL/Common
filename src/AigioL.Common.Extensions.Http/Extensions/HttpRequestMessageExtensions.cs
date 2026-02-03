#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net.Http;

public static partial class HttpRequestMessageExtensions
{
    const string DefaultCharSet = "UTF-8";

    /// <summary>
    /// 将键值对集合设置为请求的表单内容
    /// </summary>
    public static void SetContent(
        this HttpRequestMessage request,
        string? charSet,
        params IEnumerable<KeyValuePair<string?, string?>> nameValueCollection)
    {
        var content = new FormUrlEncodedContent(nameValueCollection);
        request.Content = content;

        // https://github.com/dotnet/runtime/blob/v10.0.1/src/libraries/System.Net.Http/src/System/Net/Http/FormUrlEncodedContent.cs#L23
        // ContentType 必定不会为 null 因 👆
        if (!string.IsNullOrWhiteSpace(charSet))
        {
            content.Headers.ContentType!.CharSet = charSet;
            //"UTF-8";
        }
    }

    /// <inheritdoc cref="SetContent(HttpRequestMessage, string?, IEnumerable{KeyValuePair{string?, string?}})"/>
    public static void SetContent(
        this HttpRequestMessage request,
        params IEnumerable<KeyValuePair<string?, string?>> nameValueCollection) => SetContent(request, DefaultCharSet, nameValueCollection);


    /// <inheritdoc cref="SetContent(HttpRequestMessage, IEnumerable{KeyValuePair{string?, string?}})"/>
    public static void SetContent(
        this HttpRequestMessage request,
        Dictionary<string, string?> nameValueCollection,
        string? charSet = DefaultCharSet)
    {
        IEnumerable<KeyValuePair<string?, string?>> nameValueCollection_ = nameValueCollection!;
        SetContent(request, charSet, nameValueCollection_);
    }


    /// <inheritdoc cref="SetContent(HttpRequestMessage, IEnumerable{KeyValuePair{string?, string?}})"/>
    public static void SetContent(
        this HttpRequestMessage request,
        SortedDictionary<string, string?> nameValueCollection,
        string? charSet = DefaultCharSet)
    {
        IEnumerable<KeyValuePair<string?, string?>> nameValueCollection_ = nameValueCollection!;
        SetContent(request, charSet, nameValueCollection_);
    }
}
