using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace AigioL.Common.AspNetCore.AdminCenter.Helpers;

public static partial class BMLoginHelper
{
    public static async Task<JsonWebTokenValue?> LoginAsync(
        byte[] rsaPublicKey,
        Uri baseAddress,
        string username,
        string password,
        Uri? relativeUrl = null,
        CancellationToken cancellationToken = default)
    {
        var padding = RSAEncryptionPadding.OaepSHA256;
        var rsaParameters = RSAUtils.ReadParameters(rsaPublicKey);
        using var rsa = RSA.Create(rsaParameters);

        string[] args =
        [
            Encrypt(rsa, username, padding),
            Encrypt(rsa, password, padding),
        ];

        using var client = new HttpClient();
        client.BaseAddress = baseAddress;
        using var req = new HttpRequestMessage(HttpMethod.Post, relativeUrl ?? new Uri("bm/login", UriKind.Relative));
        req.Content = JsonContent.Create(args, BMLoginHelperJsonSerializerContext.Default.StringArray);
        using var rsp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var jobj = await rsp.Content.ReadFromJsonAsync(BMLoginHelperJsonSerializerContext.Default.BMApiRspJsonWebTokenValue, cancellationToken);
        return jobj?.Content;
    }

    static string Encrypt(RSA rsa, string s, RSAEncryptionPadding padding)
    {
        var max = GetMaxByteLength(rsa, padding);

        var bytes = HttpUtility.UrlEncodeToBytes(s, Encoding.UTF8);

        var span = bytes.AsSpan();
        List<string> list = new();
        while (span.Length != 0)
        {
            Span<byte> tmp;
            if (span.Length > max)
            {
                tmp = span[..max];
                span = span[max..];
            }
            else
            {
                tmp = span;
                span = default;
            }
            var encrypt = rsa.Encrypt(tmp, padding);
            var hex = Convert.ToHexStringLower(encrypt);
            list.Add(hex);
        }

        return string.Join('\n', list);
    }

    static int GetMaxByteLength(RSA rsa, RSAEncryptionPadding padding)
    {
        int keySizeBytes = rsa.KeySize / 8;

        return padding switch
        {
            _ when padding == RSAEncryptionPadding.Pkcs1 => keySizeBytes - 11,
            _ when padding == RSAEncryptionPadding.OaepSHA1 => keySizeBytes - 2 * 20 - 2,      // 214 for 2048-bit
            _ when padding == RSAEncryptionPadding.OaepSHA256 => keySizeBytes - 2 * 32 - 2,    // 190 for 2048-bit
            _ when padding == RSAEncryptionPadding.OaepSHA384 => keySizeBytes - 2 * 48 - 2,    // 166 for 2048-bit
            _ when padding == RSAEncryptionPadding.OaepSHA512 => keySizeBytes - 2 * 64 - 2,    // 142 for 2048-bit
            _ when padding == RSAEncryptionPadding.OaepSHA3_256 => keySizeBytes - 2 * 32 - 2,    // 190 for 2048-bit
            _ when padding == RSAEncryptionPadding.OaepSHA3_384 => keySizeBytes - 2 * 48 - 2,    // 166 for 2048-bit
            _ when padding == RSAEncryptionPadding.OaepSHA3_512 => keySizeBytes - 2 * 64 - 2,    // 142 for 2048-bit
            _ => throw new NotSupportedException($"不支持的填充方案: {padding}")
        };
    }

    public sealed partial record class JsonWebTokenValue
    {
        /// <summary>
        /// 凭证有效期
        /// </summary>
        public DateTimeOffset ExpiresIn { get; set; }

        /// <summary>
        /// 当前凭证
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// 刷新凭证
        /// </summary>
        public string? RefreshToken { get; set; }
    }

    public sealed partial record class BMApiRsp<TContent>
    {
        public uint Code { get; set; }

        public string[] Messages { get; set; } = [];

        [JsonPropertyName("data")]
        public TContent? Content { get; set; }
    }
}

[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(BMLoginHelper.JsonWebTokenValue))]
[JsonSerializable(typeof(BMLoginHelper.BMApiRsp<BMLoginHelper.JsonWebTokenValue>))]
[JsonSourceGenerationOptions]
public sealed partial class BMLoginHelperJsonSerializerContext : JsonSerializerContext
{
    static BMLoginHelperJsonSerializerContext()
    {
        JsonSerializerOptions o = new();
        IJsonSerializerContext.SetDefaultOptions(o);
        Default = new BMLoginHelperJsonSerializerContext(o);
    }
}
