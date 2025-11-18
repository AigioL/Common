using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using static AigioL.Common.AspNetCore.AppCenter.Security.S96bc5bd9;

namespace AigioL.Common.AspNetCore.AppCenter.Security;

/// <summary>
/// SecurityKey 模式的中间件，通过客户端生成 AES 密钥并使用 RSA 公钥加密，再使用 AES 密钥加密请求和响应正文
/// </summary>
/// <typeparam name="TSecurityKeyOptions"></typeparam>
public sealed partial class SecurityKeyMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSecurityKeyOptions>
    where TSecurityKeyOptions : class, ISecurityKeyOptions
{
    readonly RequestDelegate _next;
    readonly TSecurityKeyOptions _options;
    //readonly ILogger _logger;
    readonly RecyclableMemoryStreamManager m = new();

#pragma warning disable IDE0290 // 使用主构造函数
    public SecurityKeyMiddleware(
#pragma warning restore IDE0290 // 使用主构造函数
        RequestDelegate next,
        //ILoggerFactory loggerFactory,
        IOptions<TSecurityKeyOptions> options)
    {
        _next = next;
        _options = options.Value;
        //_logger = loggerFactory.CreateLogger<SecurityKeyMiddleware>();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task EndHandleAsync(
        HttpContext context,
        SerializableImplType serializableImplType,
        ApiRspCode failCode)
    {
        var traceId = context.GetTraceId();
        ApiRsp apiRsp = new()
        {
            Code = unchecked((uint)failCode),
            Url = context.Request.Path,
            TraceId = traceId,
        };
        await apiRsp.WriteAsync(
            serializableImplType,
            context.Response,
            cancellationToken: context.RequestAborted);
    }

    /// <summary>
    /// Executes the middleware.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the execution of this middleware.</returns>
    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/health", StringComparison.InvariantCultureIgnoreCase))
        {
            await _next(context);
            return;
        }

        (Stream originalResponseBody, Stream originalRequestBody, Aes aes, string? responseContentType)? t = null;

        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var sk = endpoint.Metadata.OfType<RequiredSecurityKeyAttribute>().FirstOrDefault();
            if (sk != null)
            {
                // 没有请求正文时允许不加密调用需要加密的接口，兼容性
                var hasRequest = context.Request.ContentLength.HasValue &&
                    context.Request.ContentLength.Value > 0;
                StringValues contentTypeOrAccept = context.Request.ContentType;
                if (StringValues.IsNullOrEmpty(contentTypeOrAccept))
                {
                    contentTypeOrAccept = context.Request.Headers.Accept;
                }

                if (!MSMinimalApis.TryParse(contentTypeOrAccept,
                    out var isSecurity,
                    out var serializableImplType,
                    out var algorithmType,
                    out var responseContentType))
                {
                    if (hasRequest)
                    {
                        await EndHandleAsync(context, serializableImplType,
                            ApiRspCode.RequiredSecurityKey);
                        return;
                    }
                }

                if (isSecurity)
                {
                    if (algorithmType != sk.AlgorithmType)
                    {
                        await EndHandleAsync(context, serializableImplType,
                            ApiRspCode.SecurityTypeInconsistent);
                        return;
                    }

                    var code = await ReadAes(context, algorithmType, _options);
                    if (code.HasValue)
                    {
                        if (hasRequest)
                        {
                            await EndHandleAsync(context, serializableImplType,
                                code.Value);
                            return;
                        }
                    }

                    switch (serializableImplType)
                    {
                        case SerializableImplType.SystemTextJson:
                            {
                                // 将类型固定为 json，避免 RDG 生成引发 415
                                context.Request.Headers.ContentType = MediaTypeNames.JSON;
                            }
                            break;
                    }

                    if (context.Items[ApiConstants.Headers_SecurityKey] is Aes aes)
                    {
                        var originalRequestBody = context.Request.Body;
                        if (hasRequest)
                        {
                            // 启用可重新读取请求正文
                            context.Request.EnableBuffering();
                            // 将请求正文复制到内存流中
                            using var memoryStream = m.GetStream();
                            await context.Request.Body.CopyToAsync(memoryStream, context.RequestAborted);
                            memoryStream.Position = 0;
                            // 创建解密流
                            using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read, true);
                            // 创建新的正文流
                            var newRequestBody = m.GetStream();
                            // 将解密流写入新的正文流
                            await cryptoStream.CopyToAsync(newRequestBody, context.RequestAborted);
                            newRequestBody.Position = 0;
                            context.Response.RegisterForDispose(newRequestBody);
                            context.Request.Body = newRequestBody;
                        }

                        var originalResponseBody = context.Response.Body;
                        t = (originalResponseBody, originalRequestBody, aes, responseContentType);

                        // 替换响应正文为新的内存流
                        var newResponseBody = m.GetStream();
                        context.Response.RegisterForDispose(newResponseBody);
                        context.Response.Body = newResponseBody;
                    }
                    else
                    {
                        await EndHandleAsync(context, serializableImplType,
                            ApiRspCode.AesKeyIsNull);
                        return;
                    }
                }
            }
        }

        await _next(context);

        if (t.HasValue)
        {
            if (!string.IsNullOrWhiteSpace(t.Value.responseContentType))
            {
                context.Response.Headers.ContentType = t.Value.responseContentType;
            }

            // 使用原始流创建加密流并写入
            using var memoryStream = m.GetStream();
            using CryptoStream cryptoStream = new(memoryStream, t.Value.aes.CreateEncryptor(), CryptoStreamMode.Write);
            context.Response.Body.Position = 0;
            // 将响应正文流写入缓冲区
            await context.Response.Body.CopyToAsync(cryptoStream, context.RequestAborted);
            await cryptoStream.FlushFinalBlockAsync(context.RequestAborted);

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(t.Value.originalResponseBody, context.RequestAborted);

            // 恢复原始响应流
            context.Response.Body = t.Value.originalResponseBody;
        }
    }
}

file static class S96bc5bd9
{
    /// <summary>
    /// 从 HTTP 上下文中读取 AES 密钥，并存入 <see cref="HttpContext.Items"/>
    /// </summary>
    /// <returns></returns>
    internal static async Task<ApiRspCode?> ReadAes(
        HttpContext context,
        SecurityKeyAlgorithmType algorithmType,
        ISecurityKeyOptions? o) => algorithmType switch
        {
            SecurityKeyAlgorithmType.RSAWithRandomAes => await ReadAesByRSAWithRandomAes(context),
            SecurityKeyAlgorithmType.ECDHSharedKeyWithRandomIV => ReadAesByECDHSharedKeyWithRandomIV(context, o),
            _ => ApiRspCode.AesKeyIsNull,
        };

    static ApiRspCode? ReadAesByECDHSharedKeyWithRandomIV(HttpContext context, ISecurityKeyOptions? o)
    {
        var aesKey = o?.ECDH_SharedKey;
        if (aesKey == null || aesKey.Length == 0)
        {
            return ApiRspCode.AesKeyIsNull;
        }

        byte[]? iv = null;
        var sk = context.Request.Headers[ApiConstants.Headers_SecurityKeyHex];
        if (!StringValues.IsNullOrEmpty(sk))
        {
            try
            {
                // 此处键为 Hex，实际上使用 Base64 编码
                iv = Convert.FromBase64String(sk!);
            }
            catch
            {
                return ApiRspCode.RSADecryptFail;
            }
        }

        if (iv == null || iv.Length == 0)
        {
            return ApiRspCode.AesKeyIsNull;
        }

        var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = iv;
        context.Items[ApiConstants.Headers_SecurityKey] = aes;
        context.Response.RegisterForDispose(aes);
        return null;
    }

    static async Task<ApiRspCode?> ReadAesByRSAWithRandomAes(HttpContext context)
    {
        var skIsHexOrB64U = false;
        var sk = context.Request.Headers[ApiConstants.Headers_SecurityKey];
        if (StringValues.IsNullOrEmpty(sk))
        {
            sk = context.Request.Headers[ApiConstants.Headers_SecurityKeyHex];
            skIsHexOrB64U = true;
        }
        if (StringValues.IsNullOrEmpty(sk))
        {
            return ApiRspCode.AesKeyIsNull;
        }

        var appVer = await context.GetAppVerAsync();
        if (appVer == null)
        {
            return ApiRspCode.EmptyDbAppVersion;
        }

        // 从数据库表版本号中读取 RSA 私钥
        var rsaPrivateKey = appVer.PrivateKey;
        if (string.IsNullOrWhiteSpace(rsaPrivateKey))
        {
            return ApiRspCode.EmptyDbAppVersion;
        }

        var rsaPara = RSAUtils.GetRSAParameters(rsaPrivateKey);
        if (!rsaPara.HasValue)
        {
            return ApiRspCode.EmptyDbAppVersion;
        }

        byte[] cipher;
        using var rsa = RSA.Create(rsaPara.Value);
        if (skIsHexOrB64U)
        {
            cipher = Convert.FromHexString(sk!);
        }
        else
        {
            cipher = Base64Url.DecodeFromChars(sk!);
        }
        var plain = rsa.Decrypt(cipher, DefaultPadding);
        var aes = AESUtils.CreateOld(plain);
        if (aes == null)
        {
            return ApiRspCode.AesKeyIsNull;
        }

        context.Items[ApiConstants.Headers_SecurityKey] = aes;
        context.Response.RegisterForDispose(aes);
        return null;
    }

    /// <summary>
    /// 获取在特定操作系统上使用的默认加密填充模式，如果操作系统是 Android，则返回 <see cref="RSAEncryptionPadding.OaepSHA1"/>；否则返回 <see cref="RSAEncryptionPadding.OaepSHA256"/>
    /// </summary>
    public static RSAEncryptionPadding DefaultPadding
#if NET5_0_OR_GREATER
        => OperatingSystem.IsAndroid() ?
            RSAEncryptionPadding.OaepSHA1 :
#else
        =>
#endif
            RSAEncryptionPadding.OaepSHA256;
}