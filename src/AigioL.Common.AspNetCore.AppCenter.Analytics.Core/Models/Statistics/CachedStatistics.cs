namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Statistics;

public static class CachedStatistics
{
    public static CachedStatistics<T> Create<T>(T t) => new(t, DateTimeOffset.Now);
}
public sealed partial record CachedStatistics<T>(T Data, DateTimeOffset CacheTime);