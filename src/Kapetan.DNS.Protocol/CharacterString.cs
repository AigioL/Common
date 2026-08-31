using System.Text;

namespace DNS.Protocol;

/// <summary>
/// Implementation of the "character-string" non-terminal as defined in
/// RFC1035 (chapter 3.3):
///   "character-string" is a single length octet followed by that number of
///    characters. "character-string" is treated as binary information, and
///    can be up to 256 characters in length (including the length octet).
/// </summary>
public sealed class CharacterString
{
    const int MAX_SIZE = byte.MaxValue;

    byte[] data;

    public static IList<CharacterString> GetAllFromArray(byte[] message, int offset)
    {
        return GetAllFromArray(message, offset, out _);
    }

    public static IList<CharacterString> GetAllFromArray(byte[] message, int offset, out int endOffset)
    {
        var characterStrings = new List<CharacterString>();

        while (offset < message.Length)
        {
            characterStrings.Add(FromArray(message, offset, out offset));
        }

        endOffset = offset;
        return characterStrings;
    }

    public static CharacterString FromArray(byte[] message, int offset)
    {
        return FromArray(message, offset, out _);
    }

    public static CharacterString FromArray(byte[] message, int offset, out int endOffset)
    {
        if (message.Length < 1)
        {
            throw new ArgumentException("Empty message");
        }

        byte len = message[offset++];
        byte[] data = GC.AllocateUninitializedArray<byte>(len);
        Buffer.BlockCopy(message, offset, data, 0, len);
        endOffset = offset + len;
        return new CharacterString(data);
    }

    public static IList<CharacterString> FromString(string message)
    {
        return FromString(message, Encoding.ASCII);
    }

    public static IList<CharacterString> FromString(string message, Encoding encoding)
    {
        byte[] bytes = encoding.GetBytes(message);
        int size = (int)Math.Ceiling((double)bytes.Length / MAX_SIZE);
        var characterStrings = new List<CharacterString>(size);

        for (int i = 0; i < bytes.Length; i += MAX_SIZE)
        {
            int len = Math.Min(bytes.Length - i, MAX_SIZE);
            byte[] chunk = GC.AllocateUninitializedArray<byte>(len);
            Buffer.BlockCopy(bytes, i, chunk, 0, len);
            characterStrings.Add(new CharacterString(chunk));
        }

        return characterStrings;
    }

    public CharacterString(byte[] data)
    {
        if (data.Length > MAX_SIZE) Array.Resize(ref data, MAX_SIZE);
        this.data = data;
    }

    public CharacterString(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }

    public CharacterString(string data) : this(data, Encoding.ASCII) { }

    public int Size
    {
        get { return data.Length + 1; }
    }

    [Obsolete("use Write(Span<byte>) instead", true)]
    public byte[] ToArray()
    {
        byte[] result = GC.AllocateUninitializedArray<byte>(Size);
        result[0] = (byte)data.Length;
        data.CopyTo(result, 1);
        return result;
    }

    public void Write(Span<byte> result)
    {
        if (result.Length < Size)
        {
            throw new ArgumentException("Result span is too small");
        }
        result[0] = (byte)data.Length;
        data.AsSpan().CopyTo(result[1..]);
    }

    public string ToString(Encoding encoding)
    {
        return encoding.GetString(data);
    }

    public override string ToString()
    {
        return ToString(Encoding.ASCII);
    }
}
