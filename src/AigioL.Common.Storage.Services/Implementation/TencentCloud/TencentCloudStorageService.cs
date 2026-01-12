using AigioL.Common.Storage.Models.Abstractions;
using AigioL.Common.Storage.Models.Channels.TencentCloud;
using COSXML;
using COSXML.Auth;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.Storage.Services.Implementation.TencentCloud;

/// <summary>
/// 由腾讯云提供的对象存储服务实现
/// </summary>
sealed partial class TencentCloudStorageService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TSettings> :
    TencentCloudStorageService,
    IObjectStorageService
    where TSettings : class, IObjectStorageSettings
{
    readonly TSettings options;
    readonly Lazy<CosXml> lazyCosXml;

    public TencentCloudStorageService(IOptions<TSettings> options)
    {
        this.options = options.Value;
        lazyCosXml = new(() =>
        {
            ArgumentNullException.ThrowIfNull(this.options.ObjectStorageOptions.TencentCloud);
            return InitCosXml(this.options.ObjectStorageOptions.TencentCloud);
        }, true);
    }

#pragma warning disable IDE1006 // 命名样式
    CosXml cosXml => lazyCosXml.Value;
#pragma warning restore IDE1006 // 命名样式

    static CosXmlServer InitCosXml(TencentCloudOptions tencentCloudOptions)
    {
        // https://cloud.tencent.com/document/product/436/47238#0a5a6b09-0777-4d51-a090-95565985fe2c
        var region = tencentCloudOptions.Region;
        ArgumentNullException.ThrowIfNull(region);
        var secretId = tencentCloudOptions.SecretId;
        ArgumentNullException.ThrowIfNull(secretId);
        var secretKey = tencentCloudOptions.SecretKey;
        ArgumentNullException.ThrowIfNull(secretKey);

        CosXmlConfig config = new CosXmlConfig.Builder()
            .SetRegion(region) // 设置默认的地域, COS 地域的简称请参照 https://cloud.tencent.com/document/product/436/6224
            .Build();
        var durationSecond = tencentCloudOptions.GetDurationSecond(); // 每次请求签名有效时长，单位为秒
        QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
        var cosXml = new CosXmlServer(config, qCloudCredentialProvider);
        return cosXml;
    }
}

partial class TencentCloudStorageService<TSettings>
{
    public string GetCdnTempDownloadUrl(string resourceAccessPath, TimeSpan expiration)
    {
        var cdnCustomKey = options.ObjectStorageOptions.TencentCloud.CdnCustomKey;
        var cdnDomainName = options.ObjectStorageOptions.TencentCloud.CdnDomainName;

        var r = GetCdnTempDownloadUrl(resourceAccessPath, expiration, cdnCustomKey, cdnDomainName);
        return r;
    }

    //public string? GetTempDownloadUrl(string key, TimeSpan expiration)
    //{
    //    ArgumentNullException.ThrowIfNull(options.ObjectStorageOptions.TencentCloud);
    //    var region = options.ObjectStorageOptions.TencentCloud.Region;
    //    ArgumentNullException.ThrowIfNull(region);
    //    var bucket = options.ObjectStorageOptions.TencentCloud.DefaultBucket;
    //    ArgumentNullException.ThrowIfNull(bucket);
    //    var appId = options.ObjectStorageOptions.TencentCloud.AppId;
    //    ArgumentNullException.ThrowIfNull(appId);

    //    // https://cloud.tencent.com/document/product/436/47238#85551bcf-db74-4280-be0c-fe03ffe0fc95

    //    PreSignatureStruct preSignatureStruct = new PreSignatureStruct
    //    {
    //        appid = appId, // "1250000000"; //腾讯云账号 APPID
    //        region = region, // "COS_REGION"; //存储桶地域
    //        bucket = bucket, // "examplebucket-1250000000"; //存储桶
    //        key = key, // 对象键
    //        httpMethod = "GET", // HTTP 请求方法
    //        isHttps = true, // 生成 HTTPS 请求 URL
    //        signDurationSecond = (long)Math.Ceiling(expiration.TotalSeconds), // 请求签名时间
    //        headers = null, // 签名中需要校验的 header
    //        queryParameters = null // 签名中需要校验的 URL 中请求参数
    //    };
    //    var requestSignURL = cosXml.GenerateSignURL(preSignatureStruct);
    //    return requestSignURL;
    //}
}

public partial class TencentCloudStorageService
{
    public static string GetCdnTempDownloadUrl(
        string resourceAccessPath,
        TimeSpan expiration,
        string? cdnCustomKey,
        string? cdnDomainName)
    {
        // https://cloud.tencent.com/document/product/228/76110

        // 自定义密钥
        ArgumentNullException.ThrowIfNull(cdnCustomKey);

        // DomainName：CDN 域名
        ArgumentNullException.ThrowIfNull(cdnDomainName);
        var cdnDomainNameSpan = cdnDomainName.AsSpan().Trim();
        if (cdnDomainNameSpan.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
        {
            cdnDomainNameSpan = cdnDomainNameSpan[8..];
        }
        else if (cdnDomainNameSpan.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
        {
            cdnDomainNameSpan = cdnDomainNameSpan[7..];
        }

        // Filename：资源访问路径
        ReadOnlySpan<char> filename;
        bool queryExists = false;
        try
        {
            // 支持参数 资源（支持二次验证）
            var uriInfo = new Uri(resourceAccessPath);
            filename = uriInfo.PathAndQuery.AsSpan().Trim();
            if (!string.IsNullOrWhiteSpace(uriInfo.Query))
            {
                queryExists = true;
            }
        }
        catch
        {
            filename = resourceAccessPath.AsSpan().Trim();
        }

        // rand：随机字符串，0 - 100位随机字符串，由大小写字母与数字组成
        var randLength = Random.Shared.Next(1, 100);
        Span<char> rand = stackalloc char[randLength];
        GenerateRandomString(rand, "abcdefghijklmnopqrstuvwxyz0123456789");

        const string uid = "0";

        // timestamp：TypeA 使用十进制整型正数的 Unix 时间戳
        var timestamp = DateTimeOffset.Now.Add(expiration).ToUnixTimeSeconds().ToString();

        // md5hash：MD5 格式：TypeA（文件路径-timestamp-rand-uid-自定义密钥）
        Span<byte> hash = stackalloc byte[SHA256.HashSizeInBytes];
        SHA256.HashData(Encoding.UTF8.GetBytes($"{filename}-{timestamp}-{rand}-{uid}-{cdnCustomKey}"), hash);
        var hashHex = Convert.ToHexStringLower(hash);

        var typeA = $"https://{cdnDomainNameSpan}{filename}{(queryExists ? "&" : "?")}sign={timestamp}-{rand}-{uid}-{hashHex}";

        return typeA;
    }

    /// <summary>
    /// 生成随机字符串，长度为固定传入字符串
    /// </summary>
    /// <param name="chars"></param>
    /// <param name="randomChars">随机字符串字符集</param>
    /// <returns></returns>
    static void GenerateRandomString(Span<char> chars,
        string randomChars)
    {
        var random = Random.Shared;
        if (random.Next(256) % 2 == 0)
            for (var i = chars.Length - 1; i >= 0; i--) // 5 4 3 2 1 0
                EachGenerate(chars, i);
        else
            for (var i = 0; i < chars.Length; i++) // 0 1 2 3 4 5
                EachGenerate(chars, i);

        void EachGenerate(Span<char> chars, int i)
        {
            var index = random.Next(0, randomChars.Length);
            var temp = RandomCharAt(randomChars, index);
            static char RandomCharAt(string s, int index)
            {
                if (index == s.Length) index = 0;
                else if (index > s.Length) index %= s.Length;
                return s[index];
            }
            chars[i] = temp;
        }
    }
}