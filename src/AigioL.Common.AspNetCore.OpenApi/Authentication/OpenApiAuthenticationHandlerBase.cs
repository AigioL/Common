using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Encodings.Web;
using static AigioL.Common.OpenApi.Signature.Helpers.ApiSignatureHelper;

namespace AigioL.Common.AspNetCore.OpenApi.Authentication;

/// <summary>
/// 给第三方提供的 OpenApi 的身份验证处理程序
/// </summary>
public abstract partial class OpenApiAuthenticationHandlerBase(
    IOptionsMonitor<OpenApiAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) :
    AuthenticationHandler<OpenApiAuthenticationSchemeOptions>(options, logger, encoder)
{
    protected abstract ValueTask<(ReadOnlyMemory<byte> appSecret, string appName)> GetAppSecretAsync(ReadOnlyMemory<char> appAccessKey);

    protected sealed override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (StringValues.IsNullOrEmpty(Request.Headers.Authorization))
        {
            return AuthenticateResult.NoResult();
        }
        ReadOnlyMemory<char> authorization = Request.Headers.Authorization.ToString().AsMemory();
        (var hashAlgorithmTypeName, var authorizationValue, var authorizationLeft) = GetAuthorizationValue(authorization);
        if (!hashAlgorithmTypeName.HasValue || Options.IsUnSupported(hashAlgorithmTypeName.Value))
        {
            return AuthenticateResult.Fail($"不支持的签名算法: {hashAlgorithmTypeName}");
        }

        (var accessKey, var signedHeaders, var signature) = DeconstructAuthorization(authorizationValue);
        if (accessKey.Span.IsWhiteSpace())
        {
            return AuthenticateResult.Fail("缺少访问密钥");
        }
        if (signedHeaders == null || signedHeaders.Count == 0)
        {
            return AuthenticateResult.Fail("缺少签名头");
        }
        if (signature.Span.IsWhiteSpace())
        {
            return AuthenticateResult.Fail("缺少签名");
        }
        var hashSizeInBytes = hashAlgorithmTypeName.Value.GetHMACHashSizeInBytes();
        if (signature.Length != hashSizeInBytes * 2)
        {
            return AuthenticateResult.Fail($"签名长度不正确, 期望: {hashSizeInBytes * 2}, 实际: {signature.Length}");
        }

        (var appSecret, var appName) = await GetAppSecretAsync(accessKey);
        if (appSecret.IsEmpty)
        {
            return AuthenticateResult.Fail($"无效的访问密钥: {accessKey}");
        }

        var signatureBytes = ArrayPool<byte>.Shared.Rent(hashSizeInBytes);
        var signatureChars = ArrayPool<char>.Shared.Rent(hashSizeInBytes * 2);
        try
        {
            // 1. 将 RequestBody 进行哈希计算，得到 HashedRequestPayload
            Request.EnableBuffering();
            await hashAlgorithmTypeName.Value.HashDataAsync(
                Request.Body, signatureBytes, signatureChars,
                true, Context.RequestAborted);

#if DEBUG
            var debugView_HashedRequestPayload = new string(signatureChars, 0, hashSizeInBytes * 2);
#endif

            // 2. 构造规范化请求并进行哈希计算，得到 HashedCanonicalRequest
            var method = Request.Method;
            var canonicalUri = Request.Path.Value;
            var canonicalQueryString = Request.QueryString.Value;
            var headers = Request.Headers.Where(x => signedHeaders.Any(y => y.Span.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase)));

            await WriteHashedCanonicalRequest(
                method, canonicalUri, canonicalQueryString,
                headers, signatureChars.AsMemory(0, hashSizeInBytes * 2), signatureBytes,
                signatureChars, hashAlgorithmTypeName.Value,
                Context.RequestAborted);

#if DEBUG
            var debugView_HashedCanonicalRequest = new string(signatureChars, 0, hashSizeInBytes * 2);
#endif

            // 3. 构造待签名字符串，通过 HMAC 计算签名
            await WriteSignatureAsync(
                appSecret, authorizationLeft, signatureChars.AsMemory(0, hashSizeInBytes * 2),
                signatureBytes, signatureChars, hashAlgorithmTypeName.Value,
                Context.RequestAborted);

#if DEBUG
            var debugView_Signature = new string(signatureChars, 0, hashSizeInBytes * 2);
#endif

            var calcSignature = signatureChars.AsMemory(0, hashSizeInBytes * 2);
            if (signature.Span.Equals(calcSignature.Span, StringComparison.InvariantCultureIgnoreCase))
            {
                ClaimsPrincipal claimsPrincipal = new(
                    new ClaimsIdentity(
                        new OpenApiIdentity(appName, authorizationLeft.ToString(), true)));
                var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("签名不匹配");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(signatureBytes);
            ArrayPool<char>.Shared.Return(signatureChars);
        }
    }
}

file sealed record class OpenApiIdentity(string Name, string AuthenticationType, bool IsAuthenticated) : IIdentity;