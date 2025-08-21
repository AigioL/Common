namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 匹配域名地址数组
/// </summary>
public interface IMatchDomainNames : IReadOnlyMatchDomainNames
{
    /// <inheritdoc cref="IMatchDomainNames"/>
    new string MatchDomainNames { get; set; }
}

/// <inheritdoc cref="IMatchDomainNames"/>
public interface IReadOnlyMatchDomainNames
{
    /// <inheritdoc cref="IMatchDomainNames"/>
    string MatchDomainNames { get; }
}

/// <summary>
/// 对 <see cref="IMatchDomainNames"/> OR <see cref="IReadOnlyMatchDomainNames"/> 的扩展方法
/// </summary>
public static partial class MatchDomainNamesExtensions
{
    /// <summary>
    /// 获取匹配域名地址数集合
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="domainOnly">是否只返回域名</param>
    /// <returns></returns>
    public static List<string> GetMatchDomainNames(
        this IReadOnlyMatchDomainNames entity,
        bool domainOnly = true)
    {
        var matchDomainNames = entity.MatchDomainNames;
        if (string.IsNullOrWhiteSpace(matchDomainNames))
        {
            return [];
        }
        var matchDomainNamesSpan = matchDomainNames.AsSpan();
        var split = matchDomainNamesSpan.Split(';');
        List<string> results = new();
        while (split.MoveNext())
        {
            var it = matchDomainNamesSpan[split.Current].Trim();
            if (it.IsWhiteSpace())
            {
                continue;
            }
            if (domainOnly)
            {
                if (!it.Contains('.'))
                {
                    continue;
                }
            }
            results.Add(new string(it));
        }
        return results;
    }
}