using Microsoft.IdentityModel.Tokens;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

static partial class EndpointRouteBuilderExtensions
{
    public static void MapGetRSADemoHtml(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "rsa")
    {
        b.MapGet(pattern, async context =>
        {
            const bool includePrivateParameters = false; // 是否包含私钥参数
            using var rsa = RSA.Create(2048);
            var parameters = rsa.ExportParameters(includePrivateParameters);
            // https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/importKey#json_web_key
            var jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(
                new RsaSecurityKey(parameters));

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            var htmlU8_1 =
"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>RSA Web 示例</title>
    <style>
        textarea {
            min-height: 120px;
            min-width: 900px;
        }

        button {
            min-height: 30px;
            min-width: 80px;
        }
    </style>
</head>
<body>
    <p>密钥生成 C# 示例：</p>
    <textarea readonly="" style="min-height:100px">
const bool includePrivateParameters = true; // 是否包含私钥参数
using var rsa = RSA.Create(2048);
var parameters = rsa.ExportParameters(includePrivateParameters);
// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/importKey#json_web_key
var jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(
    new RsaSecurityKey(parameters));</textarea>
    <br>
    <p>请输入 RSA 密钥：</p>
    <textarea id="key" placeholder="https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/importKey#json_web_key" style="min-height:340px">

"""u8;
            context.Response.BodyWriter.Write(htmlU8_1);

            await JsonSerializer.SerializeAsync(context.Response.BodyWriter, jsonWebKey, JsonWebKey_JSC.Default.JsonWebKey, context.RequestAborted);
            var htmlU8_2 =
"""

</textarea>
    <p>输入明文进行加密：</p>
    <textarea id="input" placeholder="请输入明文…"></textarea>
    <br />
    <button id="encrypt">加密</button>
    <button id="decrypt">解密</button>
    <p>输入密文进行解密：</p>
    <textarea id="output" placeholder="请输入密文…"></textarea>
    <script type="text/javascript">
        "use strict";

        /*\
        |*|
        |*|  Base64 / binary data / UTF-8 strings utilities (#2)
        |*|
        |*|  https://developer.mozilla.org/en-US/docs/Web/API/WindowBase64/Base64_encoding_and_decoding
        |*|
        |*|  Author: madmurphy
        |*|
        \*/

        /* Array of bytes to base64 string decoding */

        function b64ToUint6(nChr) {

            return nChr > 64 && nChr < 91 ?
                nChr - 65
                : nChr > 96 && nChr < 123 ?
                    nChr - 71
                    : nChr > 47 && nChr < 58 ?
                        nChr + 4
                        : nChr === 43 ?
                            62
                            : nChr === 47 ?
                                63
                                :
                                0;

        }

        function base64DecToArr(sBase64, nBlockSize) {

            var
                sB64Enc = sBase64.replace(/[^A-Za-z0-9\+\/]/g, ""), nInLen = sB64Enc.length,
                nOutLen = nBlockSize ? Math.ceil((nInLen * 3 + 1 >>> 2) / nBlockSize) * nBlockSize : nInLen * 3 + 1 >>> 2, aBytes = new Uint8Array(nOutLen);

            for (var nMod3, nMod4, nUint24 = 0, nOutIdx = 0, nInIdx = 0; nInIdx < nInLen; nInIdx++) {
                nMod4 = nInIdx & 3;
                nUint24 |= b64ToUint6(sB64Enc.charCodeAt(nInIdx)) << 18 - 6 * nMod4;
                if (nMod4 === 3 || nInLen - nInIdx === 1) {
                    for (nMod3 = 0; nMod3 < 3 && nOutIdx < nOutLen; nMod3++, nOutIdx++) {
                        aBytes[nOutIdx] = nUint24 >>> (16 >>> nMod3 & 24) & 255;
                    }
                    nUint24 = 0;
                }
            }

            return aBytes;
        }

        /* Base64 string to array encoding */

        function uint6ToB64(nUint6) {

            return nUint6 < 26 ?
                nUint6 + 65
                : nUint6 < 52 ?
                    nUint6 + 71
                    : nUint6 < 62 ?
                        nUint6 - 4
                        : nUint6 === 62 ?
                            43
                            : nUint6 === 63 ?
                                47
                                :
                                65;

        }

        function base64EncArr(aBytes) {

            var eqLen = (3 - (aBytes.length % 3)) % 3, sB64Enc = "";

            for (var nMod3, nLen = aBytes.length, nUint24 = 0, nIdx = 0; nIdx < nLen; nIdx++) {
                nMod3 = nIdx % 3;
                /* Uncomment the following line in order to split the output in lines 76-character long: */
                /*
                if (nIdx > 0 && (nIdx * 4 / 3) % 76 === 0) { sB64Enc += "\r\n"; }
                */
                nUint24 |= aBytes[nIdx] << (16 >>> nMod3 & 24);
                if (nMod3 === 2 || aBytes.length - nIdx === 1) {
                    sB64Enc += String.fromCharCode(uint6ToB64(nUint24 >>> 18 & 63), uint6ToB64(nUint24 >>> 12 & 63), uint6ToB64(nUint24 >>> 6 & 63), uint6ToB64(nUint24 & 63));
                    nUint24 = 0;
                }
            }

            return eqLen === 0 ?
                sB64Enc
                :
                sB64Enc.substring(0, sB64Enc.length - eqLen) + (eqLen === 1 ? "=" : "==");

        }

        /* UTF-8 array to DOMString and vice versa */

        function UTF8ArrToStr(aBytes) {

            var sView = "";

            for (var nPart, nLen = aBytes.length, nIdx = 0; nIdx < nLen; nIdx++) {
                nPart = aBytes[nIdx];
                sView += String.fromCharCode(
                    nPart > 251 && nPart < 254 && nIdx + 5 < nLen ? /* six bytes */
                        /* (nPart - 252 << 30) may be not so safe in ECMAScript! So...: */
                        (nPart - 252) * 1073741824 + (aBytes[++nIdx] - 128 << 24) + (aBytes[++nIdx] - 128 << 18) + (aBytes[++nIdx] - 128 << 12) + (aBytes[++nIdx] - 128 << 6) + aBytes[++nIdx] - 128
                        : nPart > 247 && nPart < 252 && nIdx + 4 < nLen ? /* five bytes */
                            (nPart - 248 << 24) + (aBytes[++nIdx] - 128 << 18) + (aBytes[++nIdx] - 128 << 12) + (aBytes[++nIdx] - 128 << 6) + aBytes[++nIdx] - 128
                            : nPart > 239 && nPart < 248 && nIdx + 3 < nLen ? /* four bytes */
                                (nPart - 240 << 18) + (aBytes[++nIdx] - 128 << 12) + (aBytes[++nIdx] - 128 << 6) + aBytes[++nIdx] - 128
                                : nPart > 223 && nPart < 240 && nIdx + 2 < nLen ? /* three bytes */
                                    (nPart - 224 << 12) + (aBytes[++nIdx] - 128 << 6) + aBytes[++nIdx] - 128
                                    : nPart > 191 && nPart < 224 && nIdx + 1 < nLen ? /* two bytes */
                                        (nPart - 192 << 6) + aBytes[++nIdx] - 128
                                        : /* nPart < 127 ? */ /* one byte */
                                        nPart
                );
            }

            return sView;

        }

        function strToUTF8Arr(sDOMStr) {

            var aBytes, nChr, nStrLen = sDOMStr.length, nArrLen = 0;

            /* mapping... */

            for (var nMapIdx = 0; nMapIdx < nStrLen; nMapIdx++) {
                nChr = sDOMStr.charCodeAt(nMapIdx);
                nArrLen += nChr < 0x80 ? 1 : nChr < 0x800 ? 2 : nChr < 0x10000 ? 3 : nChr < 0x200000 ? 4 : nChr < 0x4000000 ? 5 : 6;
            }

            aBytes = new Uint8Array(nArrLen);

            /* transcription... */

            for (var nIdx = 0, nChrIdx = 0; nIdx < nArrLen; nChrIdx++) {
                nChr = sDOMStr.charCodeAt(nChrIdx);
                if (nChr < 128) {
                    /* one byte */
                    aBytes[nIdx++] = nChr;
                } else if (nChr < 0x800) {
                    /* two bytes */
                    aBytes[nIdx++] = 192 + (nChr >>> 6);
                    aBytes[nIdx++] = 128 + (nChr & 63);
                } else if (nChr < 0x10000) {
                    /* three bytes */
                    aBytes[nIdx++] = 224 + (nChr >>> 12);
                    aBytes[nIdx++] = 128 + (nChr >>> 6 & 63);
                    aBytes[nIdx++] = 128 + (nChr & 63);
                } else if (nChr < 0x200000) {
                    /* four bytes */
                    aBytes[nIdx++] = 240 + (nChr >>> 18);
                    aBytes[nIdx++] = 128 + (nChr >>> 12 & 63);
                    aBytes[nIdx++] = 128 + (nChr >>> 6 & 63);
                    aBytes[nIdx++] = 128 + (nChr & 63);
                } else if (nChr < 0x4000000) {
                    /* five bytes */
                    aBytes[nIdx++] = 248 + (nChr >>> 24);
                    aBytes[nIdx++] = 128 + (nChr >>> 18 & 63);
                    aBytes[nIdx++] = 128 + (nChr >>> 12 & 63);
                    aBytes[nIdx++] = 128 + (nChr >>> 6 & 63);
                    aBytes[nIdx++] = 128 + (nChr & 63);
                } else /* if (nChr <= 0x7fffffff) */ {
                    /* six bytes */
                    aBytes[nIdx++] = 252 + (nChr >>> 30);
                    aBytes[nIdx++] = 128 + (nChr >>> 24 & 63);
                    aBytes[nIdx++] = 128 + (nChr >>> 18 & 63);
                    aBytes[nIdx++] = 128 + (nChr >>> 12 & 63);
                    aBytes[nIdx++] = 128 + (nChr >>> 6 & 63);
                    aBytes[nIdx++] = 128 + (nChr & 63);
                }
            }

            return aBytes;

        }
    </script>
    <script type="text/javascript">
        class RSA {
            async getCryptokey() {
                const includePrivateParameters = false;
                let key = document.getElementById('key').value;
                let keystr = JSON.parse(key);
                keystr.alg = 'RSA-OAEP-256';
                keystr.key_ops = [includePrivateParameters ? "decrypt" : "encrypt"];
                let cryptokey = await crypto.subtle.importKey(
                    "jwk",
                    keystr,
                    {
                        name: "RSA-OAEP",
                        hash: { name: "SHA-256" }
                    },
                    true,
                    keystr.key_ops
                );
                return cryptokey;
            }
            /**
             * 补全 Base64 末尾 = 符号
             */
            padString(s) {
                let segmentLength = 4;
                let stringLength = s.length;
                let diff = stringLength % segmentLength;

                if (!diff) {
                    return s;
                }

                let padLength = segmentLength - diff;
                let paddedStringLength = stringLength + padLength;

                while (padLength--) {
                    s += '=';
                }

                return s;
            }
            /**
             * Base64Url 解码
             */
            base64UrlDecToArr(sBase64, nBlockSize) {
                sBase64 = this.padString(sBase64)
                    .replace(/\-/g, "+")
                    .replace(/_/g, "/");
                return base64DecToArr(sBase64, nBlockSize);
            }
            /**
             * Base64Url 编码
             */
            base64UrlEncArr(aBytes) {
                var sBase64 = base64EncArr(aBytes);
                return sBase64
                    .replace(/=/g, "")
                    .replace(/\+/g, "-")
                    .replace(/\//g, "_");
            }
            async encrypt(s) {
                let enc = new TextEncoder();
                let encoded = enc.encode(s);
                let ciphertext = await crypto.subtle.encrypt(
                    {
                        name: "RSA-OAEP",
                        hash: { name: "SHA-256" }
                    },
                    await this.getCryptokey(),
                    encoded
                );
                let buffer = new Uint8Array(ciphertext);
                let r = this.base64UrlEncArr(buffer);
                //let r = buffer.toBase64({ alphabet: "base64url" });
                return r;
            }
            async decrypt(s) {
                let encoded = this.base64UrlDecToArr(s);
                //let encoded = Uint8Array.fromBase64(s, { alphabet: "base64url" })
                let ciphertext = await crypto.subtle.decrypt(
                    {
                        name: "RSA-OAEP",
                        hash: { name: "SHA-256" }
                    },
                    await this.getCryptokey(),
                    encoded
                );
                let buffer = new Uint8Array(ciphertext);
                var input = UTF8ArrToStr(buffer);
            }
        }
        rsa = new RSA();
        document.getElementById('encrypt').addEventListener('click', async () => {
            let input = document.getElementById('input').value;
            if (input) {
                let output = await rsa.encrypt(input);
                document.getElementById('output').value = output;
            }
        });
        document.getElementById('decrypt').addEventListener('click', async () => {
            let input = document.getElementById('output').value;
            if (input) {
                let output = await rsa.decrypt(input);
                document.getElementById('input').value = output;
            }
        });
    </script>
</body>
</html>
"""u8;
            context.Response.BodyWriter.Write(htmlU8_2);
            await context.Response.BodyWriter.FlushAsync(context.RequestAborted);
        }).AllowAnonymous();
    }
}

[JsonSerializable(typeof(JsonWebKey))]
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true)]
sealed partial class JsonWebKey_JSC : JsonSerializerContext
{
    static JsonWebKey_JSC()
    {
        // https://github.com/dotnet/runtime/issues/94135
        JsonSerializerOptions o = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 不转义字符！！！
            AllowTrailingCommas = true,

            #region JsonSerializerDefaults.Web https://github.com/dotnet/runtime/blob/v9.0.7/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonSerializerOptions.cs#L172-L174

            PropertyNameCaseInsensitive = true, // 忽略大小写
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // 驼峰命名
            NumberHandling = JsonNumberHandling.AllowReadingFromString, // 允许从字符串读取数字

            #endregion

            WriteIndented = true,

        };
        Default = new JsonWebKey_JSC(o);
    }
}