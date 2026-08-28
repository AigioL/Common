#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net;

/// <summary>
/// SNI 自定义值表达式
/// <list type="bullet">
/// <item>@domain 变量表示取域名值</item>
/// <item>@ipadress 变量表示取 IP 地址</item>
/// <item>@random 变量表示取随机值</item>
/// </list>
/// </summary>
public readonly struct TlsSniPattern
{
    readonly string? _value;

    /// <summary>
    /// 获取表示式值
    /// </summary>
    public readonly string Value => _value ?? string.Empty;

    /// <summary>
    /// 变量表示取域名值
    /// </summary>
    public const string DomainValue = "@domain";

    /// <summary>
    /// 变量表示取 Ip 地址
    /// </summary>
    public const string IPAddressValue = "@ipaddress";

    /// <summary>
    /// 变量表示取随机值
    /// </summary>
    public const string RandomValue = "@random";

    /// <summary>
    /// 无 SNI
    /// </summary>
    public static TlsSniPattern None { get; } = new TlsSniPattern(default);

    /// <summary>
    /// 域名 SNI
    /// </summary>
    public static TlsSniPattern Domain { get; } = new TlsSniPattern(DomainValue);

    /// <summary>
    /// IP 值的 SNI
    /// </summary>
    public static TlsSniPattern IPAddress { get; } = new TlsSniPattern(IPAddressValue);

    /// <summary>
    /// 随机值的 SNI
    /// </summary>
    public static TlsSniPattern Random { get; } = new TlsSniPattern(RandomValue);

    /// <summary>
    /// SNI 自定义值表达式
    /// </summary>
    /// <param name="value">表示式值</param>
#pragma warning disable IDE0290 // 使用主构造函数
    public TlsSniPattern(string? value)
    {
        _value = value;

    }
#pragma warning restore IDE0290 // 使用主构造函数

    //public TlsSniPattern(ReadOnlyMemory<char> value)
    //{
    //    _value2 = value;
    //}

    /// <summary>
    /// 更新域名
    /// </summary>
    public readonly TlsSniPattern WithDomain(string domain)
    {
        if (string.IsNullOrEmpty(_value))
        {
            return None;
        }
        var value = _value.Replace(DomainValue, domain, StringComparison.InvariantCultureIgnoreCase);
        return new TlsSniPattern(value);
    }

    /// <summary>
    /// 更新 IP 地址
    /// </summary>
    public readonly TlsSniPattern WithIPAddress(IPAddress address)
    {
        if (string.IsNullOrEmpty(_value))
        {
            return None;
        }
        var value = _value.Replace(IPAddressValue, address.ToString(), StringComparison.InvariantCultureIgnoreCase);
        return new TlsSniPattern(value);
    }

    /// <summary>
    /// 更新随机数
    /// </summary>
    public readonly TlsSniPattern WithRandom()
    {
        if (string.IsNullOrEmpty(_value))
        {
            return None;
        }
        var value = _value.Replace(RandomValue,
#if NETCOREAPP3_0_OR_GREATER
            Environment.TickCount64.ToString(),
#else
            Environment.TickCount.ToString(),
#endif
            StringComparison.InvariantCultureIgnoreCase);
        return new TlsSniPattern(value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (_value != null)
        {
            return _value;
        }
        //else if (!_value2.IsEmpty)
        //{
        //    return _value = _value2.ToString();
        //}

        return string.Empty;
    }
}