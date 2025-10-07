using AigioL.Common.Essentials.Storage;
using System.Collections.Immutable;
using System.Text;

namespace AigioL.Common.Essentials.ApplicationModel.Implementation;

/// <summary>
/// https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/VersionTracking/VersionTracking.shared.cs#L198
/// </summary>
sealed partial class VersionTrackingImplementation : IVersionTracking
{
    const string versionsKey = "VersionTracking.Versions";
    const string buildsKey = "VersionTracking.Builds";

    readonly IPreferences preferences;

    readonly string sharedName;

    Dictionary<string, ImmutableArray<string>> versionTrail = null!;

    string LastInstalledVersion => versionTrail[versionsKey].LastOrDefault() ?? string.Empty;

    string LastInstalledBuild => versionTrail[buildsKey].LastOrDefault() ?? string.Empty;

    internal VersionTrackingImplementation(
        IPreferences preferences,
        string packageName,
        string versionString,
        string buildString)
    {
        sharedName = $"{packageName}.microsoft.maui.essentials.versiontracking";

        this.preferences = preferences;
        CurrentVersion = versionString;
        CurrentBuild = buildString;

        Track();
    }

    public void Track()
    {
        if (versionTrail != null)
            return;

        InitVersionTracking();
    }

    /// <summary>
    /// Initialize VersionTracking module, load data and track current version
    /// </summary>
    /// <remarks>
    /// For internal use. Usually only called once in production code, but multiple times in unit tests
    /// </remarks>
    internal void InitVersionTracking()
    {
        IsFirstLaunchEver = !preferences.ContainsKey(versionsKey, sharedName) || !preferences.ContainsKey(buildsKey, sharedName);
        if (IsFirstLaunchEver)
        {
            versionTrail = new(StringComparer.Ordinal)
            {
                { versionsKey, ImmutableArray.Create<string>() },
                { buildsKey, ImmutableArray.Create<string>() }
            };
        }
        else
        {
            versionTrail = new(StringComparer.Ordinal)
            {
                { versionsKey, ReadHistory(versionsKey).ToImmutableArray() },
                { buildsKey, ReadHistory(buildsKey).ToImmutableArray() }
            };
        }

        IsFirstLaunchForCurrentVersion = !versionTrail[versionsKey].Contains(CurrentVersion) || CurrentVersion != LastInstalledVersion;
        if (IsFirstLaunchForCurrentVersion)
        {
            // Avoid duplicates and move current version to end of list if already present
            versionTrail[versionsKey] = versionTrail[versionsKey].RemoveAll(v => v == CurrentVersion).Add(CurrentVersion);
        }

        IsFirstLaunchForCurrentBuild = !versionTrail[buildsKey].Contains(CurrentBuild) || CurrentBuild != LastInstalledBuild;
        if (IsFirstLaunchForCurrentBuild)
        {
            // Avoid duplicates and move current build to end of list if already present
            versionTrail[buildsKey] = versionTrail[buildsKey].RemoveAll(b => b == CurrentBuild).Add(CurrentBuild);
        }

        if (IsFirstLaunchForCurrentVersion || IsFirstLaunchForCurrentBuild)
        {
            WriteHistory(versionsKey, versionTrail[versionsKey]);
            WriteHistory(buildsKey, versionTrail[buildsKey]);
        }
    }

    public bool IsFirstLaunchEver { get; private set; }

    public bool IsFirstLaunchForCurrentVersion { get; private set; }

    public bool IsFirstLaunchForCurrentBuild { get; private set; }

    public string CurrentVersion { get; }

    public string CurrentBuild { get; }

    public string? PreviousVersion => GetPrevious(versionsKey);

    public string? PreviousBuild => GetPrevious(buildsKey);

    public string? FirstInstalledVersion => versionTrail[versionsKey].FirstOrDefault();

    public string? FirstInstalledBuild => versionTrail[buildsKey].FirstOrDefault();

    public ImmutableArray<string> VersionHistory => [.. versionTrail[versionsKey]];

    public ImmutableArray<string> BuildHistory => [.. versionTrail[buildsKey]];

    public bool IsFirstLaunchForVersion(string version)
        => CurrentVersion == version && IsFirstLaunchForCurrentVersion;

    public bool IsFirstLaunchForBuild(string build)
        => CurrentBuild == build && IsFirstLaunchForCurrentBuild;

    public string GetStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("VersionTracking");
        sb.AppendLine($"  IsFirstLaunchEver:              {IsFirstLaunchEver}");
        sb.AppendLine($"  IsFirstLaunchForCurrentVersion: {IsFirstLaunchForCurrentVersion}");
        sb.AppendLine($"  IsFirstLaunchForCurrentBuild:   {IsFirstLaunchForCurrentBuild}");
        sb.AppendLine();
        sb.AppendLine($"  CurrentVersion:                 {CurrentVersion}");
        sb.AppendLine($"  PreviousVersion:                {PreviousVersion}");
        sb.AppendLine($"  FirstInstalledVersion:          {FirstInstalledVersion}");
        sb.AppendLine($"  VersionHistory:                 [{string.Join(", ", VersionHistory)}]");
        sb.AppendLine();
        sb.AppendLine($"  CurrentBuild:                   {CurrentBuild}");
        sb.AppendLine($"  PreviousBuild:                  {PreviousBuild}");
        sb.AppendLine($"  FirstInstalledBuild:            {FirstInstalledBuild}");
        sb.AppendLine($"  BuildHistory:                   [{string.Join(", ", BuildHistory)}]");
        return sb.ToString();
    }

    string[] ReadHistory(string key)
        => preferences.Get<string?>(key, null, sharedName)?.Split(['|'], StringSplitOptions.RemoveEmptyEntries) ?? [];

    void WriteHistory(string key, IEnumerable<string> history)
        => preferences.Set(key, string.Join("|", history), sharedName);

    string? GetPrevious(string key)
    {
        var trail = versionTrail[key];
        return (trail.Length >= 2) ? trail[^2] : null;
    }
}
