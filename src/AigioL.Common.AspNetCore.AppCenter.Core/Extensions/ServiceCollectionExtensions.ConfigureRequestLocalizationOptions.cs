using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Collections.Immutable;
using System.Globalization;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加本地化配置 <see cref="RequestLocalizationOptions"/>
    /// </summary>
    public static IServiceCollection ConfigureRequestLocalizationOptions(
        this IServiceCollection services,
        HashSet<string>? supportedCultures = null,
        ImmutableDictionary<string, string>? mapping = null)
    {
        supportedCultures ??= AcceptLanguageHeaderRequestCultureProvider2.SupportedCultures;
        var supportedCultureInfos = supportedCultures
            .Select(culture => new CultureInfo(culture))
            .ToArray();
        return services.Configure<RequestLocalizationOptions>(options =>
        {
            // 设置默认文化
            options.DefaultRequestCulture = new RequestCulture(supportedCultureInfos.First());

            // 设置支持的文化
            options.SupportedCultures = options.SupportedUICultures = supportedCultureInfos;

            options.AddInitialRequestCultureProvider(new AcceptLanguageHeaderRequestCultureProvider2(mapping)
            {
                Options = options,
            });
        });
    }
}

/// <summary>
/// https://github.com/dotnet/aspnetcore/blob/v10.0.0-rc.2.25502.107/src/Middleware/Localization/src/AcceptLanguageHeaderRequestCultureProvider.cs
/// </summary>
file sealed class AcceptLanguageHeaderRequestCultureProvider2 : RequestCultureProvider
{
    static readonly ImmutableDictionary<string, string> DefaultMapping = new Dictionary<string, string>()
    {
        { "zh-Hant", "zh-HK" },
        { "zh-MO", "zh-HK" },
        { "en-US", "en" },
        { "en-GB", "en" },
        { "de", "de-DE" },
        { "fr", "fr-FR" },
        { "pl", "pl-PL" },
        { "tr", "tr-TR" },
        { "ja", "ja-JP" },
        { "ko", "ko-KR" },
        { "id", "id-ID" },
        { "it", "it-IT" },
        { "th", "th-TH" },
        { "ru", "ru-RU" },
    }.ToImmutableDictionary();

    internal static readonly HashSet<string> SupportedCultures =
    [
        "zh-CN",
        "zh-SG",
        "zh-Hans",
        "zh-TW",
        "zh-HK",
        "en",
        "de-DE",
        "es-ES",
        "fr-FR",
        "pl-PL",
        "pt-BR",
        "tr-TR",
        "ja-JP",
        "ko-KR",
        "hi-IN",
        "id-ID",
        "it-IT",
        "th-TH",
        "ru-RU",
    ];

    readonly ImmutableDictionary<string, string> mapping;

    internal AcceptLanguageHeaderRequestCultureProvider2(ImmutableDictionary<string, string>? mapping)
    {
        this.mapping = mapping ?? DefaultMapping;
    }

    /// <summary>
    /// The maximum number of values in the Accept-Language header to attempt to create a <see cref="System.Globalization.CultureInfo"/>
    /// from for the current request.
    /// Defaults to <c>3</c>.
    /// </summary>
    const int MaximumAcceptLanguageHeaderValuesToTry = 3;

    StringSegment GetValue(StringWithQualityHeaderValue v)
    {
        foreach (var it in mapping)
        {
            if (v.Value.Equals(it.Key, StringComparison.InvariantCultureIgnoreCase))
            {
                return it.Value;
            }
        }
        return v.Value;
    }

    /// <inheritdoc />
    public sealed override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var acceptLanguageHeader = httpContext.Request.GetTypedHeaders().AcceptLanguage;

        if (acceptLanguageHeader == null || acceptLanguageHeader.Count == 0)
        {
            return NullProviderCultureResult;
        }

        var languages = acceptLanguageHeader.AsEnumerable();

        if (MaximumAcceptLanguageHeaderValuesToTry > 0)
        {
            // We take only the first configured number of languages from the header and then order those that we
            // attempt to parse as a CultureInfo to mitigate potentially spinning CPU on lots of parse attempts.
            languages = languages.Take(MaximumAcceptLanguageHeaderValuesToTry);
        }

        var orderedLanguages = languages.OrderByDescending(h => h, StringWithQualityHeaderValueComparer.QualityComparer)
            .Select(GetValue).ToList();

        if (orderedLanguages.Count > 0)
        {
            return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(orderedLanguages));
        }

        return NullProviderCultureResult;
    }
}