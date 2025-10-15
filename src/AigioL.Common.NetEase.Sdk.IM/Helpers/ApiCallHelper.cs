using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace AigioL.Common.NetEase.Sdk.IM.Helpers;

/// <summary>
/// API 调用助手类
/// <para>https://doc.yunxin.163.com/messaging/server-apis/jk3MzY2MTI?platform=server</para>
/// </summary>
internal partial class ApiCallHelper
{
    /// <summary>
    /// 设置请求头参数
    /// <para>https://doc.yunxin.163.com/messaging/server-apis/jk3MzY2MTI?platform=server#%e8%af%b7%e6%b1%82%e5%a4%b4%e5%8f%82%e6%95%b0</para>
    /// </summary>
    public void SetRequestHeaders(
        HttpRequestMessage request,
        string appKey,
        string appSecret)
    {
        // AigioL.Common.SmsSender.Services.Implementation.SmsSender.Channels.NetEaseCloud.SmsSenderProvider

        var nonce = Nonce();
        var curTime = CurTime();
        var checkSum = CheckSum(appSecret, nonce, curTime);
        //var content = new FormUrlEncodedContent(args);
        //content.Headers.ContentType = new("application/x-www-form-urlencoded", "utf-8");
    }

    /// <summary>
    /// 当前 UTC 时间戳，秒数
    /// </summary>
    /// <returns></returns>
    static string CurTime() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    /// <summary>
    /// 生成唯一标识符（最大长度 128 个字符）
    /// </summary>
    static string Nonce() => Guid.NewGuid().ToString("N") + DateTime.Now.Ticks;

    static string CheckSum(string appSecret, string nonce, string curTime)
    {
        string result;
        var len = Encoding.UTF8.GetMaxByteCount(appSecret.Length + nonce.Length + curTime.Length);
        var b = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            len = 0;
            if (!Encoding.UTF8.TryGetBytes(appSecret, b, out var bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode AppSecret to UTF-8.");
            }
            len += bytesWritten;
            if (!Encoding.UTF8.TryGetBytes(nonce, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode Nonce to UTF-8.");
            }
            len += bytesWritten;
            if (!Encoding.UTF8.TryGetBytes(curTime, b.AsSpan(len), out bytesWritten))
            {
                throw new InvalidOperationException("Failed to encode CurTime to UTF-8.");
            }
            len += bytesWritten;
            Span<byte> hash = new byte[SHA1.HashSizeInBytes];
            SHA1.HashData(b.AsSpan(0, len), hash);
            result = Convert.ToHexString(hash);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(b);
        }
        return result;
    }
}
