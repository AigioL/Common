using AigioL.Common.TencentCloud.Sdk.IM.Models.Abstractions;
using Microsoft.IO;
using System.Buffers;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AigioL.Common.TencentCloud.Sdk.IM.Helpers;

/// <summary>
/// https://cloud.tencent.com/document/product/269/32688
/// <para>https://github.com/AigioL/tls-sig-api-v2-cs/blob/master/tls-sig-api-v2-cs/TLSSigAPIv2.cs</para>
/// </summary>
public static partial class TLSSigAPIv2
{
    public static string GenUserSig(
        this ITLSSigAPIv2AppIdWithKey opt,
        string userid,
        int expire = 180 * 86400)
    {
        return opt.GenUserSig(userid, expire, null);
    }

    public static string GenPrivateMapKey(
        this ITLSSigAPIv2AppIdWithKey opt,
        string userid,
        int expire,
        uint roomid,
        uint privilegeMap)
    {
        byte[] userbuf = opt.GenUserBuf(userid, roomid, expire, privilegeMap, 0, "");
        return opt.GenUserSig(userid, expire, userbuf);
    }

    public static string GenPrivateMapKeyWithStringRoomID(
        this ITLSSigAPIv2AppIdWithKey opt,
        string userid,
        int expire,
        string roomstr,
        uint privilegeMap)
    {
        byte[] userbuf = opt.GenUserBuf(userid, 0, expire, privilegeMap, 0, roomstr);
        return opt.GenUserSig(userid, expire, userbuf);
    }

    static string GenUserSig(
         this ITLSSigAPIv2AppIdWithKey opt,
         string userid,
         int expire,
         byte[]? userbuf)
    {
        var sdkappid = opt.SdkAppId;
        var currTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var userBufEnabled = userbuf != null;
        var base64UserBuf = userBufEnabled ? Convert.ToBase64String(userbuf!) : null;
        var base64sig = opt.CalcHMACSHA256(userid, currTime, expire, base64UserBuf);

        using var stream = recyclableMemoryStreamManager.GetStream();
        stream.Write(
"""
{"TLS.ver":"2.0","TLS.identifier":"
"""u8);
        stream.Write(JsonEncodedText.Encode(userid).EncodedUtf8Bytes);
        stream.Write(
"""
","TLS.sdkappid":
"""u8);
        stream.Write(uint.Parse(sdkappid));
        stream.Write(
"""
,"TLS.expire":
"""u8);
        stream.Write(expire);
        stream.Write(
"""
,"TLS.time":
"""u8);
        stream.Write(currTime);
        stream.Write(
"""
,"TLS.sig":"
"""u8);
        stream.Write(JsonEncodedText.Encode(base64sig).EncodedUtf8Bytes);
        if (userBufEnabled)
        {
            stream.Write(
"""
","TLS.userbuf":"
"""u8);
            stream.Write(JsonEncodedText.Encode(base64UserBuf!).EncodedUtf8Bytes);
        }
        stream.Write(
"""
"}
"""u8);

#if DEBUG
        //stream.Position = 0;
        //var j = Encoding.UTF8.GetString(stream.GetBuffer());
        //Console.WriteLine(j);
#endif

        var r = CompressStreamWithBase64StringReplace(stream);
        return r;
    }

    public static byte[] GenUserBuf(
        this ITLSSigAPIv2AppIdWithKey opt,
        string account,
        uint dwAuthID,
        int dwExpTime,
        uint dwPrivilegeMap,
        uint dwAccountType,
        string roomStr)
    {
        var sdkappid = uint.Parse(opt.SdkAppId);

        int length = 1 + 2 + account.Length + 20;
        int offset = 0;
        if (roomStr.Length > 0)
            length = length + 2 + roomStr.Length;
        byte[] userBuf = new byte[length];

        if (roomStr.Length > 0)
            userBuf[offset++] = 1;
        else
            userBuf[offset++] = 0;

        userBuf[offset++] = (byte)((account.Length & 0xFF00) >> 8);
        userBuf[offset++] = (byte)(account.Length & 0x00FF);

#if NETCOREAPP3_0_OR_GREATER
        byte[]? accountByteR = null;
        try
        {
            int expectedByteCount = Encoding.UTF8.GetMaxByteCount(account.Length);
            Span<byte> accountByte = expectedByteCount <= StackallocByteThreshold ?
                stackalloc byte[StackallocByteThreshold] :
                (accountByteR = ArrayPool<byte>.Shared.Rent(expectedByteCount));

            var actualByteCount = Encoding.UTF8.GetBytes(account, accountByte);
            accountByte = accountByte[..actualByteCount];
            accountByte.CopyTo(userBuf.AsSpan(offset));
            offset += account.Length;
        }
        finally
        {
            if (accountByteR is not null)
            {
                ArrayPool<byte>.Shared.Return(accountByteR);
            }
        }
#else
        byte[] accountByte = Encoding.UTF8.GetBytes(account);
        accountByte.CopyTo(userBuf, offset);
        offset += roomStr.Length;
#endif

        //dwSdkAppid
        userBuf[offset++] = (byte)((sdkappid & 0xFF000000) >> 24);
        userBuf[offset++] = (byte)((sdkappid & 0x00FF0000) >> 16);
        userBuf[offset++] = (byte)((sdkappid & 0x0000FF00) >> 8);
        userBuf[offset++] = (byte)(sdkappid & 0x000000FF);

        //dwAuthId
        userBuf[offset++] = (byte)((dwAuthID & 0xFF000000) >> 24);
        userBuf[offset++] = (byte)((dwAuthID & 0x00FF0000) >> 16);
        userBuf[offset++] = (byte)((dwAuthID & 0x0000FF00) >> 8);
        userBuf[offset++] = (byte)(dwAuthID & 0x000000FF);

        //time_t now = time(0);
        //uint32_t expire = now + dwExpTime;
        long expire = dwExpTime + (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        userBuf[offset++] = (byte)((expire & 0xFF000000) >> 24);
        userBuf[offset++] = (byte)((expire & 0x00FF0000) >> 16);
        userBuf[offset++] = (byte)((expire & 0x0000FF00) >> 8);
        userBuf[offset++] = (byte)(expire & 0x000000FF);

        //dwPrivilegeMap     
        userBuf[offset++] = (byte)((dwPrivilegeMap & 0xFF000000) >> 24);
        userBuf[offset++] = (byte)((dwPrivilegeMap & 0x00FF0000) >> 16);
        userBuf[offset++] = (byte)((dwPrivilegeMap & 0x0000FF00) >> 8);
        userBuf[offset++] = (byte)(dwPrivilegeMap & 0x000000FF);

        //dwAccountType
        userBuf[offset++] = (byte)((dwAccountType & 0xFF000000) >> 24);
        userBuf[offset++] = (byte)((dwAccountType & 0x00FF0000) >> 16);
        userBuf[offset++] = (byte)((dwAccountType & 0x0000FF00) >> 8);
        userBuf[offset++] = (byte)(dwAccountType & 0x000000FF);

        if (roomStr.Length > 0)
        {
            userBuf[offset++] = (byte)((roomStr.Length & 0xFF00) >> 8);
            userBuf[offset++] = (byte)(roomStr.Length & 0x00FF);

#if NETCOREAPP3_0_OR_GREATER
            byte[]? roomStrByteR = null;
            try
            {
                int expectedByteCount = Encoding.UTF8.GetMaxByteCount(account.Length);
                Span<byte> roomStrByte = expectedByteCount <= StackallocByteThreshold ?
                    stackalloc byte[StackallocByteThreshold] :
                    (roomStrByteR = ArrayPool<byte>.Shared.Rent(expectedByteCount));

                var actualByteCount = Encoding.UTF8.GetBytes(roomStr, roomStrByte);
                roomStrByte = roomStrByte[..actualByteCount];
                roomStrByte.CopyTo(userBuf.AsSpan(offset));
                offset += account.Length;
            }
            finally
            {
                if (roomStrByteR is not null)
                {
                    ArrayPool<byte>.Shared.Return(roomStrByteR);
                }
            }
#else
            byte[] roomStrByte = Encoding.UTF8.GetBytes(roomStr);
            roomStrByte.CopyTo(userBuf, offset);
            offset += roomStr.Length;
#endif
        }
        return userBuf;
    }

    static string CompressStreamWithBase64StringReplace(Stream sourceStream)
    {
        using var streamOut = recyclableMemoryStreamManager.GetStream();
        {
            using DeflateStream streamZOut = new DeflateStream(streamOut, CompressionMode.Compress, true);
            sourceStream.Position = 0;
            sourceStream.CopyTo(streamZOut);
            streamZOut.Flush();
        }
        streamOut.Position = 0;
        byte[] buffer =
#if NET5_0_OR_GREATER
            GC.AllocateUninitializedArray<byte>(unchecked((int)streamOut.Length));
#else
            new byte[unchecked((int)streamOut.Length)];
#endif
#if NET7_0_OR_GREATER
        streamOut.ReadExactly(buffer, 0, buffer.Length);
#else
        streamOut.Read(buffer, 0, buffer.Length);
#endif

        var b64Len = (buffer.Length / 3 + (buffer.Length % 3 == 0 ? 0 : 3)) * 4;
        char[] array = ArrayPool<char>.Shared.Rent(b64Len);
        try
        {
            var b64Len2 = Convert.ToBase64CharArray(buffer, 0, buffer.Length, array, 0);
            var chars = array.AsSpan(0, b64Len2);
            var newChars = new char[chars.Length];
            chars.CopyTo(newChars);

            for (int i = 0; i < newChars.Length; i++)
            {
                switch (newChars[i])
                {
                    case '+':
                        newChars[i] = '*';
                        break;
                    case '/':
                        newChars[i] = '-';
                        break;
                    case '=':
                        newChars[i] = '_';
                        break;
                }
            }

            return new string(newChars);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<char>.Shared.Return(array);
            }
        }
    }

    static readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager = new();

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v9.0.8/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    const int StackallocByteThreshold = 256;

    static void Write(this Stream stream, string? str)
    {
        if (str == null)
        {
            return;
        }

#if NETCOREAPP3_0_OR_GREATER
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(str.Length);

        byte[]? array = null;
        Span<byte> utf8Bytes = expectedByteCount <= StackallocByteThreshold ?
            stackalloc byte[StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount));

        try
        {
            var actualByteCount = Encoding.UTF8.GetBytes(str, utf8Bytes);
            utf8Bytes = utf8Bytes[..actualByteCount];
            stream.Write(utf8Bytes);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
#else
        stream.Write(Encoding.UTF8.GetBytes(str));
#endif
    }

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
    static void Write(this Stream stream, byte[] buffer) => stream.Write(buffer, 0, buffer.Length);
#endif

    static void Write(this Stream stream, long num)
    {
#if NET8_0_OR_GREATER
        var len = Encoding.UTF8.GetMaxByteCount(20);
        Span<byte> u8 = stackalloc byte[len];
        if (num.TryFormat(u8, out var bytesWritten))
        {
            stream.Write(u8[..bytesWritten]);
            return;
        }
#endif
        stream.Write(Encoding.UTF8.GetBytes(num.ToString()));
    }

    static void Write(this Stream stream, int num)
    {
#if NET8_0_OR_GREATER
        var len = Encoding.UTF8.GetMaxByteCount(11);
        Span<byte> u8 = stackalloc byte[len];
        if (num.TryFormat(u8, out var bytesWritten))
        {
            stream.Write(u8[..bytesWritten]);
            return;
        }
#endif
        stream.Write(Encoding.UTF8.GetBytes(num.ToString()));
    }

    static string CalcHMACSHA256(
        this ITLSSigAPIv2AppIdWithKey opt,
        string identifier,
        long currTime,
        int expire,
        string? base64UserBuf = null)
    {
        using var stream = recyclableMemoryStreamManager.GetStream();

        stream.Write("TLS.identifier:"u8);
        stream.Write(identifier);
        stream.Write("\n"u8);

        stream.Write("TLS.sdkappid:"u8);
        stream.Write(opt.SdkAppId);
        stream.Write("\n"u8);

        stream.Write("TLS.time:"u8);
        stream.Write(currTime);
        stream.Write("\n"u8);

        stream.Write("TLS.expire:"u8);
        stream.Write(expire);
        stream.Write("\n"u8);

        if (base64UserBuf != null)
        {
            stream.Write("TLS.userbuf:"u8);
            stream.Write(base64UserBuf);
            stream.Write("\n"u8);
        }

        stream.Position = 0;
        int keyU8Len = Encoding.UTF8.GetMaxByteCount(opt.Key.Length);
        byte[]? keyU8 = null;
        try
        {
#if NETCOREAPP3_0_OR_GREATER
            Span<byte> keyU8Span = keyU8Len <= StackallocByteThreshold ?
            stackalloc byte[StackallocByteThreshold] :
            (keyU8 = ArrayPool<byte>.Shared.Rent(keyU8Len));
            var keyU8ActualLen = Encoding.UTF8.GetBytes(opt.Key, keyU8Span);
            keyU8Span = keyU8Span[..keyU8ActualLen];
#else
            keyU8 = ArrayPool<byte>.Shared.Rent(keyU8Len);
            var keyU8ActualLen = Encoding.UTF8.GetBytes(opt.Key, 0, opt.Key.Length, keyU8, 0);
#endif
#if NET7_0_OR_GREATER
            Span<byte> hash = stackalloc byte[HMACSHA256.HashSizeInBytes];
            HMACSHA256.HashData(keyU8Span, stream, hash);
#else
            var key = keyU8ActualLen == keyU8.Length ? keyU8 : keyU8.AsSpan(0, keyU8ActualLen).ToArray();
            using HMACSHA256 h = new(key);
            var hash = h.ComputeHash(stream);
#endif
            var r = Convert.ToBase64String(hash);
            return r;
        }
        finally
        {
            if (keyU8 is not null)
            {
                ArrayPool<byte>.Shared.Return(keyU8);
            }
        }
    }
}