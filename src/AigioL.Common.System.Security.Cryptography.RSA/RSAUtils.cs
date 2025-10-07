using System.Buffers.Binary;
using System.Buffers.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace System.Security.Cryptography;

public static partial class RSAUtils
{
    public static RSAParameters ReadParameters(Stream s, long? position = 0)
    {
        try
        {
            if (position.HasValue && s.Position != position.Value)
            {
                s.Position = position.Value;
            }
        }
        catch
        {
        }

        int len;
        Span<byte> buffer = stackalloc byte[sizeof(int)];

        len = F7f482c3.ReadInt32(s, buffer, true);
        byte[]? d;
        if (len > 0)
        {
            d = new byte[len];
            s.ReadExactly(d);
        }
        else
        {
            d = null;
        }

        len = F7f482c3.ReadInt32(s, buffer, false);
        byte[]? dp;
        if (len > 0)
        {
            dp = new byte[len];
            s.ReadExactly(dp);
        }
        else
        {
            dp = null;
        }

        len = F7f482c3.ReadInt32(s, buffer, false);
        byte[]? dq;
        if (len > 0)
        {
            dq = new byte[len];
            s.ReadExactly(dq);
        }
        else
        {
            dq = null;
        }

        len = F7f482c3.ReadInt32(s, buffer, true);
        byte[]? exponent;
        if (len > 0)
        {
            exponent = new byte[len];
            s.ReadExactly(exponent);
        }
        else
        {
            exponent = null;
        }

        len = F7f482c3.ReadInt32(s, buffer, false);
        byte[]? inverseQ;
        if (len > 0)
        {
            inverseQ = new byte[len];
            s.ReadExactly(inverseQ);
        }
        else
        {
            inverseQ = null;
        }

        len = F7f482c3.ReadInt32(s, buffer, false);
        byte[]? modulus;
        if (len > 0)
        {
            modulus = new byte[len];
            s.ReadExactly(modulus);
        }
        else
        {
            modulus = null;
        }

        len = F7f482c3.ReadInt32(s, buffer, false);
        byte[]? p;
        if (len > 0)
        {
            p = new byte[len];
            s.ReadExactly(p);
        }
        else
        {
            p = null;
        }

        len = F7f482c3.ReadInt32(s, buffer, true);
        byte[]? q;
        if (len > 0)
        {
            q = new byte[len];
            s.ReadExactly(q);
        }
        else
        {
            q = null;
        }

        RSAParameters parameters = new()
        {
            D = d,
            DQ = dq,
            DP = dp,
            Exponent = exponent,
            InverseQ = inverseQ,
            Modulus = modulus,
            P = p,
            Q = q
        };
        return parameters;
    }

    public static RSAParameters ReadParameters(ReadOnlySpan<byte> b)
    {
        int len;
        int start = 0;

        len = F7f482c3.ReadInt32(b, ref start, true);
        byte[]? d;
        if (len > 0)
        {
            d = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            d = null;
        }

        len = F7f482c3.ReadInt32(b, ref start, false);
        byte[]? dp;
        if (len > 0)
        {
            dp = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            dp = null;
        }

        len = F7f482c3.ReadInt32(b, ref start, false);
        byte[]? dq;
        if (len > 0)
        {
            dq = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            dq = null;
        }

        len = F7f482c3.ReadInt32(b, ref start, true);
        byte[]? exponent;
        if (len > 0)
        {
            exponent = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            exponent = null;
        }

        len = F7f482c3.ReadInt32(b, ref start, false);
        byte[]? inverseQ;
        if (len > 0)
        {
            inverseQ = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            inverseQ = null;
        }

        len = F7f482c3.ReadInt32(b, ref start, false);
        byte[]? modulus;
        if (len > 0)
        {
            modulus = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            modulus = null;
        }

        len = F7f482c3.ReadInt32(b, ref start, false);
        byte[]? p;
        if (len > 0)
        {
            p = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            p = null;
        }

        len = F7f482c3.ReadInt32(b, ref start, true);
        byte[]? q;
        if (len > 0)
        {
            q = b.Slice(start, len).ToArray();
            start += len;
        }
        else
        {
            q = null;
        }

        RSAParameters parameters = new()
        {
            D = d,
            DQ = dq,
            DP = dp,
            Exponent = exponent,
            InverseQ = inverseQ,
            Modulus = modulus,
            P = p,
            Q = q
        };
        return parameters;
    }

    public static int GetParametersLength(RSAParameters parameters)
    {
        int len = 0;
        len += parameters.D?.Length ?? 0;
        len += parameters.DP?.Length ?? 0;
        len += parameters.DQ?.Length ?? 0;
        len += parameters.Exponent?.Length ?? 0;
        len += parameters.InverseQ?.Length ?? 0;
        len += parameters.Modulus?.Length ?? 0;
        len += parameters.P?.Length ?? 0;
        len += parameters.Q?.Length ?? 0;
        return len + (sizeof(int) * 8); // 每个参数前面都有一个 int 长度
    }

    public static void WriteParameters(Stream s, RSAParameters parameters)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];

        F7f482c3.WriteBytes(s, buffer, parameters.D, true);
        F7f482c3.WriteBytes(s, buffer, parameters.DP, false);
        F7f482c3.WriteBytes(s, buffer, parameters.DQ, false);
        F7f482c3.WriteBytes(s, buffer, parameters.Exponent, true);
        F7f482c3.WriteBytes(s, buffer, parameters.InverseQ, false);
        F7f482c3.WriteBytes(s, buffer, parameters.Modulus, false);
        F7f482c3.WriteBytes(s, buffer, parameters.P, false);
        F7f482c3.WriteBytes(s, buffer, parameters.Q, true);

        s.Flush();
    }

    public static void WriteParameters(Span<byte> b, RSAParameters parameters)
    {
        int bytesWritten;
        Span<byte> buffer = stackalloc byte[sizeof(int)];

        bytesWritten = F7f482c3.WriteBytes(b, buffer, parameters.D, true);
        b = b[bytesWritten..];
        bytesWritten = F7f482c3.WriteBytes(b, buffer, parameters.DP, false);
        b = b[bytesWritten..];
        bytesWritten = F7f482c3.WriteBytes(b, buffer, parameters.DQ, false);
        b = b[bytesWritten..];
        bytesWritten = F7f482c3.WriteBytes(b, buffer, parameters.Exponent, true);
        b = b[bytesWritten..];
        bytesWritten = F7f482c3.WriteBytes(b, buffer, parameters.InverseQ, false);
        b = b[bytesWritten..];
        bytesWritten = F7f482c3.WriteBytes(b, buffer, parameters.Modulus, false);
        b = b[bytesWritten..];
        bytesWritten = F7f482c3.WriteBytes(b, buffer, parameters.P, false);
        b = b[bytesWritten..];
        F7f482c3.WriteBytes(b, buffer, parameters.Q, true);
    }

    [Obsolete("use RSAParameters")]
    public sealed class Parameters
    {
        [JsonPropertyName("z")]
        public string? D { get; set; }

        [JsonPropertyName("x")]
        public string? DP { get; set; }

        [JsonPropertyName("c")]
        public string? DQ { get; set; }

        [JsonPropertyName("v")]
        public string? Exponent { get; set; }

        [JsonPropertyName("b")]
        public string? InverseQ { get; set; }

        [JsonPropertyName("n")]
        public string? Modulus { get; set; }

        [JsonPropertyName("m")]
        public string? P { get; set; }

        [JsonPropertyName("a")]
        public string? Q { get; set; }

        public static JsonTypeInfo<Parameters> GetJsonTypeInfo() => ParametersJsonSerializerContext.Default.Parameters;

        public static implicit operator RSAParameters(Parameters? rsaUtilsParameters)
        {
            ArgumentNullException.ThrowIfNull(rsaUtilsParameters);
            return new()
            {
                D = rsaUtilsParameters.D != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.D) : null,
                DP = rsaUtilsParameters.DP != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.DP) : null,
                DQ = rsaUtilsParameters.DQ != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.DQ) : null,
                Exponent = rsaUtilsParameters.Exponent != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.Exponent) : null,
                InverseQ = rsaUtilsParameters.InverseQ != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.InverseQ) : null,
                Modulus = rsaUtilsParameters.Modulus != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.Modulus) : null,
                P = rsaUtilsParameters.P != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.P) : null,
                Q = rsaUtilsParameters.Q != null ? Base64Url.DecodeFromChars(rsaUtilsParameters.Q) : null,
            };
        }
    }
}

[Obsolete("use RSAParameters")]
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    AllowTrailingCommas = true
    )]
[JsonSerializable(typeof(RSAUtils.Parameters))]
sealed partial class ParametersJsonSerializerContext : JsonSerializerContext
{
}


static class F7f482c3
{
    /// <summary>
    /// 从流中读取一个 <see cref="int"/>
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="buffer"></param>
    /// <param name="isLittleEndian"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static int ReadInt32(Stream stream, Span<byte> buffer, bool isLittleEndian = true)
    {
        // 读取 (4 字节)
        buffer = buffer[..sizeof(int)];
        var len = stream.Read(buffer);
        if (len != sizeof(int))
        {
            throw new ArgumentOutOfRangeException(nameof(len), len, null);
        }
        return isLittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(buffer) : BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    internal static int ReadInt32(ReadOnlySpan<byte> b, ref int start, bool isLittleEndian = true)
    {
        const int len = sizeof(int);
        var buffer = b.Slice(start, len);
        start += len;
        return isLittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(buffer) : BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    internal static void WriteBytes(Stream stream, Span<byte> buffer, byte[]? bytes, bool isLittleEndian = true)
    {
        int len;
        if (bytes != null)
        {
            len = bytes.Length;
        }
        else
        {
            len = 0;
        }
        if (isLittleEndian)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, len);
        }
        else
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer, len);
        }
        stream.Write(buffer);
        if (len > 0 && bytes != null)
        {
            stream.Write(bytes);
        }
    }

    internal static int WriteBytes(Span<byte> b, Span<byte> buffer, byte[]? bytes, bool isLittleEndian = true)
    {
        int bytesWritten = 0;
        int len;
        if (bytes != null)
        {
            len = bytes.Length;
        }
        else
        {
            len = 0;
        }
        if (isLittleEndian)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, len);
        }
        else
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer, len);
        }
        if (buffer.Length != sizeof(int))
        {
            buffer = buffer[..sizeof(int)];
        }
        buffer.CopyTo(b);
        bytesWritten += buffer.Length;
        if (len > 0 && bytes != null)
        {
            bytes.CopyTo(b[bytesWritten..]);
            bytesWritten += bytes.Length;
        }
        return bytesWritten;
    }
}