using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace DNS.Protocol.ResourceRecords;

public sealed partial class TextResourceRecord : BaseResourceRecord
{
    /// Regular expression that matches the attribute name/value.
    /// The first unescaped equal sign is the name/value delimiter.
    [GeneratedRegex(@"^([ -~]*?)(?<!`)=([ -~]*)$")]
    private static partial Regex PATTERN_TXT_RECORD();

    /// Regular expression that matches unescaped leading/trailing whitespace.
    [GeneratedRegex(@"^\s+|((?<!`)\s)+$")]
    private static partial Regex PATTERN_TRIM_NAME();

    /// Regular expression that matches unescaped characters.
    [GeneratedRegex(@"([`=])")]
    private static partial Regex PATTERN_ESCAPE();

    /// Regular expression that matches escaped characters.
    [GeneratedRegex(@"`([`=\s])")]
    private static partial Regex PATTERN_UNESCAPE();

    static string Trim(string value) => PATTERN_TRIM_NAME().Replace(value, string.Empty);
    static string Escape(string value) => PATTERN_ESCAPE().Replace(value, "`$1");
    static string Unescape(string value) => PATTERN_UNESCAPE().Replace(value, "$1");

    static IResourceRecord Create(Domain domain, IList<CharacterString> characterStrings, TimeSpan ttl)
    {
        byte[] data = new byte[characterStrings.Sum(c => c.Size)];
        int offset = 0;

        foreach (CharacterString characterString in characterStrings)
        {
            characterString.Write(data.AsSpan(offset));
            offset += characterString.Size;
        }

        return new ResourceRecord(domain, data, RecordType.TXT, RecordClass.IN, ttl);
    }

    static IList<CharacterString> FormatAttributeNameValue(string attributeName, string attributeValue)
    {
        return CharacterString.FromString($"{Escape(attributeName)}={attributeValue}");
    }

    public TextResourceRecord(IResourceRecord record) :
        base(record)
    {
        TextData = CharacterString.GetAllFromArray(Data, 0);
    }

    public TextResourceRecord(Domain domain, IList<CharacterString> characterStrings,
            TimeSpan ttl = default) : base(Create(domain, characterStrings, ttl))
    {
        TextData = new ReadOnlyCollection<CharacterString>(characterStrings);
    }

    public TextResourceRecord(Domain domain, string attributeName, string attributeValue,
            TimeSpan ttl = default) :
            this(domain, FormatAttributeNameValue(attributeName, attributeValue), ttl)
    { }

    public IList<CharacterString> TextData { get; }

    public KeyValuePair<string?, string> Attribute
    {
        get
        {
            var text = ToStringTextData();
            Match match = PATTERN_TXT_RECORD().Match(text);

            if (match.Success)
            {
                var attributeName = (match.Groups[1].Length > 0) ?
                    Unescape(Trim(match.Groups[1].ToString())) : null;
                var attributeValue = Unescape(match.Groups[2].ToString());
                return new KeyValuePair<string?, string>(attributeName, attributeValue);
            }
            else
            {
                return new KeyValuePair<string?, string>(null, Unescape(text));
            }
        }
    }

    public string ToStringTextData()
    {
        return ToStringTextData(Encoding.ASCII);
    }

    public string ToStringTextData(Encoding encoding)
    {
        return string.Join(null, TextData.Select(c => c.ToString(encoding)));
    }

    public override string ToString()
    {
        return Stringify<TextResourceRecord>().Add(nameof(TextData), (object)ToStringTextData()).ToString();
    }
}
