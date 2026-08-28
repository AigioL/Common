using System.Buffers;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static AigioL.Common.Net.ReverseProxy.Constants.GeneralConstants;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Net;

/// <summary>
/// 域名表达式
/// <para>* 表示除 . 之外任意 0 到多个字符</para>
/// </summary>
public sealed class DomainPattern : IComparable<DomainPattern>
{
    readonly ImmutableArray<Regex> regexs;
    readonly string domainPattern;

    /// <summary>
    /// 排序
    /// </summary>
    public long Order { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainPattern"/> class.
    /// </summary>
    public DomainPattern(string domainPattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domainPattern);

        this.domainPattern = domainPattern;

        var items = domainPattern.Split(GeneralSeparator, StringSplitOptions.RemoveEmptyEntries);

        regexs = [.. items.Select(s =>
        {
            var isRegex = s.StartsWith('/');
            if (isRegex)
            {
                return new Regex(s[1..], RegexOptions.IgnoreCase);
            }
            else
            {
                var regexPattern = Regex.Escape(s).Replace(@"\*", @"[^\.]*");
                return new Regex($"^{regexPattern}", RegexOptions.IgnoreCase);
            }
        })];
    }

    /// <summary>
    /// 与目标比较
    /// </summary>
    public int CompareTo(DomainPattern? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (Order < other.Order)
        {
            return -1;
        }
        else if (Order > other.Order)
        {
            return 1;
        }

        var spanX = domainPattern.AsSpan();
        var spanY = other.domainPattern.AsSpan();

        List<Range> segmentsX = new();
        var segmentsXSplit = spanX.Split('.');
        while (segmentsXSplit.MoveNext())
        {
            segmentsX.Add(segmentsXSplit.Current);
        }
        List<Range> segmentsY = new();
        var segmentsYSplit = spanY.Split('.');
        while (segmentsYSplit.MoveNext())
        {
            segmentsY.Add(segmentsYSplit.Current);
        }
        var value = segmentsX.Count - segmentsY.Count;
        if (value != 0)
        {
            return value;
        }

        for (var i = segmentsX.Count - 1; i >= 0; i--)
        {
            var x = spanX[segmentsX[i]];
            var y = spanY[segmentsY[i]];

            value = Compare(x, y);
            if (value == 0)
            {
                continue;
            }
            return value;
        }

        return 0;
    }

    /// <summary>
    /// 比较两个分段
    /// </summary>
    static int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y)
    {
        char[]? arrayX = null;
        Span<char> valueX = x.Length <= StackallocCharThreshold ?
            stackalloc char[StackallocCharThreshold] :
            (arrayX = ArrayPool<char>.Shared.Rent(x.Length)).AsSpan(0, x.Length);
        char[]? arrayY = null;
        Span<char> valueY = y.Length <= StackallocCharThreshold ?
            stackalloc char[StackallocCharThreshold] :
            (arrayY = ArrayPool<char>.Shared.Rent(y.Length)).AsSpan(0, y.Length);
        try
        {
            x.Replace(valueX, '*', char.MaxValue);
            y.Replace(valueY, '*', char.MaxValue);
        }
        finally
        {
            if (arrayX is not null)
            {
                ArrayPool<char>.Shared.Return(arrayX);
            }
            if (arrayY is not null)
            {
                ArrayPool<char>.Shared.Return(arrayY);
            }
        }
        return valueX.CompareTo(valueY, StringComparison.CurrentCulture);
    }

    /// <summary>
    /// 是否与指定字符串匹配
    /// </summary>
    public bool IsMatch(string value) => regexs.Any(it => it.IsMatch(value));

    /// <summary>
    /// 是否与指定字符串匹配
    /// </summary>
    public bool IsMatch(ReadOnlySpan<char> value)
    {
        for (var i = 0; i < regexs.Length; i++)
        {
            var it = regexs[i];
            if (it.IsMatch(value))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 是否与指定域名匹配
    /// </summary>
    public bool IsMatchOnlyDomain(string domain)
    {
        try
        {
            if (domain.Contains('/'))
            {
                Uri uri = new(domain);
                domain = uri.Host;
            }
        }
        catch
        {
        }
        var result = IsMatch(domain);
        return result;
    }

    /// <summary>
    /// 是否与指定域名匹配
    /// </summary>
    public bool IsMatchOnlyDomain(ReadOnlySpan<char> domain)
    {
        try
        {
            if (domain.Contains('/'))
            {
                Uri uri = new(domain.ToString());
                domain = uri.Host;
            }
        }
        catch
        {
        }
        var result = IsMatch(domain);
        return result;
    }

    /// <inheritdoc/>
    public override string ToString() => domainPattern;

    public ReadOnlySpan<char> AsSpan() => domainPattern;
}
