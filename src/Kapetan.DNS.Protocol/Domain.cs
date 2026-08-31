using DNS.Protocol.Utils;
using System.Buffers;
using System.Net;
using System.Text;

namespace DNS.Protocol;

public sealed class Domain : IComparable<Domain>
{
    const byte ASCII_UPPERCASE_FIRST = 65;
    const byte ASCII_UPPERCASE_LAST = 90;
    const byte ASCII_LOWERCASE_FIRST = 97;
    const byte ASCII_LOWERCASE_LAST = 122;
    const byte ASCII_UPPERCASE_MASK = 223;

    readonly byte[][] labels;

    public static Domain FromString(string domain)
    {
        return new Domain(domain);
    }

    public static Domain FromString(ReadOnlyMemory<char> domain)
    {
        return new Domain(domain);
    }

    public static Domain FromArray(ReadOnlySpan<byte> message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static Domain FromArray(ReadOnlySpan<byte> message, int offset, out int endOffset)
    {
        var labels = new List<byte[]>();
        bool endOffsetAssigned = false;
        endOffset = 0;
        byte lengthOrPointer;
        HashSet<int> visitedOffsetPointers = new HashSet<int>();

        while ((lengthOrPointer = message[offset++]) > 0)
        {
            // Two highest bits are set (pointer)
            if (lengthOrPointer.GetBitValueAt(6, 2) == 3)
            {
                if (!endOffsetAssigned)
                {
                    endOffsetAssigned = true;
                    endOffset = offset + 1;
                }

                ushort pointer = lengthOrPointer.GetBitValueAt(0, 6);
                offset = (pointer << 8) | message[offset];

                if (visitedOffsetPointers.Contains(offset))
                {
                    throw new ArgumentException("Compression pointer loop detected");
                }
                visitedOffsetPointers.Add(offset);

                continue;
            }

            if (lengthOrPointer.GetBitValueAt(6, 2) != 0)
            {
                throw new ArgumentException("Unexpected bit pattern in label length");
            }

            byte length = lengthOrPointer;
            byte[] label = GC.AllocateUninitializedArray<byte>(length);
            message.Slice(offset, length).CopyTo(label);

            labels.Add(label);

            offset += length;
        }

        if (!endOffsetAssigned)
        {
            endOffset = offset;
        }

        return new Domain(labels.ToArray());
    }

    public static Domain PointerName(IPAddress ip)
    {
        return new Domain(FormatReverseIP(ip));
    }

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/Common/src/System/Net/IPAddressParserStatics.cs#L9
    /// </summary>
    const int IPv6AddressBytes = 16;

    static string FormatReverseIP(IPAddress ip)
    {
        Span<byte> address = stackalloc byte[IPv6AddressBytes];
        if (!ip.TryWriteBytes(address, out var bytesWritten))
        {
            address = ip.GetAddressBytes();
        }
        else
        {
            address = address[..bytesWritten];
        }

        StringBuilder builder;
        if (address.Length == 4)
        {
            const string suffix_ipv4 = ".in-addr.arpa";
            builder = new(suffix_ipv4.Length + (4 * 3) + 3);
            for (int i = address.Length - 1; i >= 0; i--)
            {
                builder.Append(address[i]);
                if (i > 0)
                {
                    builder.Append('.');
                }
            }
            builder.Append(suffix_ipv4);
            return builder.ToString();
        }

        Span<byte> nibbles = stackalloc byte[address.Length * 2];

        for (int i = 0, j = 0; i < address.Length; i++, j = 2 * i)
        {
            byte b = address[i];

            nibbles[j] = b.GetBitValueAt(4, 4);
            nibbles[j + 1] = b.GetBitValueAt(0, 4);
        }

        const string suffix_ipv6 = ".ip6.arpa";
        builder = new(suffix_ipv6.Length + (nibbles.Length * 2) + (nibbles.Length - 1));
        for (int i = nibbles.Length - 1; i >= 0; i--)
        {
            builder.Append(nibbles[i].ToString("x"));
            if (i > 0)
            {
                builder.Append('.');
            }
        }
        builder.Append(suffix_ipv6);
        return builder.ToString();
    }

    static bool IsASCIIAlphabet(byte b)
    {
        return (ASCII_UPPERCASE_FIRST <= b && b <= ASCII_UPPERCASE_LAST) ||
            (ASCII_LOWERCASE_FIRST <= b && b <= ASCII_LOWERCASE_LAST);
    }

    static int CompareTo(byte a, byte b)
    {
        if (IsASCIIAlphabet(a) && IsASCIIAlphabet(b))
        {
            a &= ASCII_UPPERCASE_MASK;
            b &= ASCII_UPPERCASE_MASK;
        }

        return a - b;
    }

    static int CompareTo(byte[] a, byte[] b)
    {
        int length = Math.Min(a.Length, b.Length);

        for (int i = 0; i < length; i++)
        {
            int v = CompareTo(a[i], b[i]);
            if (v != 0) return v;
        }

        return a.Length - b.Length;
    }

    public Domain(byte[][] labels)
    {
        this.labels = labels;
    }

    public Domain(IEnumerable<string> labels, Encoding encoding)
    {
        this.labels = [.. labels.Select(encoding.GetBytes)];
    }

    public Domain(string domain) : this(domain.AsMemory()) { }

    public Domain(ReadOnlyMemory<char> domain)
    {
        List<byte[]> labels = new();
        var split = domain.Span.Split('.');
        while (split.MoveNext())
        {
            var it = Encoding.UTF8.GetBytes(new ReadOnlySequence<char>(domain[split.Current]));
            labels.Add(it);
        }
        this.labels = [.. labels];
    }

    public Domain(string[] labels) : this(labels, Encoding.ASCII) { }

    public int Size
    {
        get { return labels.Sum(l => l.Length) + labels.Length + 1; }
    }

    public byte[] ToArray()
    {
        byte[] result = GC.AllocateUninitializedArray<byte>(Size);
        int offset = 0;

        foreach (byte[] label in labels)
        {
            result[offset++] = (byte)label.Length;
            label.CopyTo(result, offset);
            offset += label.Length;
        }

        result[offset] = 0;
        return result;
    }

    public void Write(Span<byte> result)
    {
        if (result.Length < Size)
        {
            throw new ArgumentException("Result span is too small");
        }
        int offset = 0;

        foreach (byte[] label in labels)
        {
            result[offset++] = (byte)label.Length;
            label.CopyTo(result[offset..]);
            offset += label.Length;
        }

        result[offset] = 0;
    }

    public string ToString(Encoding encoding)
    {
        return string.Join('.', labels.Select(label => encoding.GetString(label)));
    }

    public override string ToString()
    {
        return ToString(Encoding.ASCII);
    }

    public int CompareTo(Domain? other)
    {
        int length = Math.Min(labels.Length, other?.labels?.Length ?? 0);

        for (int i = 0; i < length; i++)
        {
            int v = CompareTo(labels[i], other?.labels[i] ?? []);
            if (v != 0) return v;
        }

        return labels.Length - (other?.labels?.Length ?? 0);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (!(obj is Domain))
        {
            return false;
        }

        return CompareTo(obj as Domain) == 0;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            foreach (byte[] label in labels)
            {
                foreach (byte b in label)
                {
                    hash = hash * 31 + (IsASCIIAlphabet(b) ? b & ASCII_UPPERCASE_MASK : b);
                }
            }

            return hash;
        }
    }
}
