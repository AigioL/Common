using AigioL.Common.Essentials.ApplicationModel;
using AigioL.Common.Essentials.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Reflection;
using static AigioL.Common.UnitTest.Program;

namespace AigioL.Common.UnitTest.Essentials;

public sealed partial class VersionTracking_Tests : BaseUnitTest
{
    IVersionTracking VersionTracking { get; }

    IPreferences Preferences { get; }
}

#pragma warning disable IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
[Category("VersionTracking")]
partial class VersionTracking_Tests // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/test/DeviceTests/Tests/VersionTracking_Tests.cs
{
    const string versionsKey = "VersionTracking.Versions";
    const string buildsKey = "VersionTracking.Builds";
    readonly string sharedName;

    readonly ITestOutputHelper output;

    public VersionTracking_Tests(ITestOutputHelper output)
    {
        this.output = output;

        Preferences = Program.Services.GetRequiredService<IPreferences>();
        VersionTracking = Program.Services.GetRequiredService<IVersionTracking>();

        sharedName = VersionTracking.GetType()
            .GetField("sharedName", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(VersionTracking)
            ?.ToString()!;
        ArgumentNullException.ThrowIfNull(sharedName);
    }

    [Fact]
    public void First_Launch_Ever()
    {
        VersionTracking.Track();
        Preferences.Clear(sharedName);

        VersionTracking.InitVersionTracking();

        Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
        Assert.True(VersionTracking.IsFirstLaunchEver);
        Assert.True(VersionTracking.IsFirstLaunchForCurrentVersion);
        Assert.True(VersionTracking.IsFirstLaunchForCurrentBuild);
    }

    [Fact]
    public void First_Launch_For_Version()
    {
        //VersionTracking.Track();
        Preferences.Set(versionsKey, string.Join("|", new string[] { "0.8.0", "0.9.0", "1.0.0" }), sharedName);
        Preferences.Set(buildsKey, string.Join("|", new string[] { currentBuild }), sharedName);

        VersionTracking.InitVersionTracking();

        Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
        Assert.Equal("1.0.0", VersionTracking.PreviousVersion);
        Assert.Equal("0.8.0", VersionTracking.FirstInstalledVersion);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.True(VersionTracking.IsFirstLaunchForCurrentVersion);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);

        VersionTracking.InitVersionTracking();

        Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
        Assert.Equal("1.0.0", VersionTracking.PreviousVersion);
        Assert.Equal("0.8.0", VersionTracking.FirstInstalledVersion);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);
    }

    [Fact]
    public void First_Launch_For_Build()
    {
        VersionTracking.Track();
        Preferences.Set(versionsKey, string.Join("|", new string[] { currentVersion }), sharedName);
        Preferences.Set(buildsKey, string.Join("|", new string[] { "10", "20" }), sharedName);

        VersionTracking.InitVersionTracking();

        Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
        Assert.Equal("20", VersionTracking.PreviousBuild);
        Assert.Equal("10", VersionTracking.FirstInstalledBuild);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
        Assert.True(VersionTracking.IsFirstLaunchForCurrentBuild);

        VersionTracking.InitVersionTracking();

        Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
        Assert.Equal("20", VersionTracking.PreviousBuild);
        Assert.Equal("10", VersionTracking.FirstInstalledBuild);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);
    }

    [Fact]
    public void First_Launch_After_Downgrade()
    {
        VersionTracking.Track();
        Preferences.Set(versionsKey, string.Join("|", new string[] { currentVersion, "1.0.2", "1.0.3" }), sharedName);

        VersionTracking.InitVersionTracking();
        output.WriteLine(VersionTracking.GetStatus());

        Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
        Assert.Equal("1.0.3", VersionTracking.PreviousVersion);
        Assert.Equal("1.0.2", VersionTracking.FirstInstalledVersion);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.True(VersionTracking.IsFirstLaunchForCurrentVersion);

        VersionTracking.InitVersionTracking();

        Assert.Equal(currentVersion, VersionTracking.CurrentVersion);
        Assert.Equal("1.0.3", VersionTracking.PreviousVersion);
        Assert.Equal("1.0.2", VersionTracking.FirstInstalledVersion);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
    }

    [Fact]
    public void First_Launch_After_Build_Downgrade()
    {
        VersionTracking.Track();
        Preferences.Set(versionsKey, string.Join("|", new string[] { currentVersion }), sharedName);
        Preferences.Set(buildsKey, string.Join("|", new string[] { currentBuild, "10", "20" }), sharedName);

        VersionTracking.InitVersionTracking();
        output.WriteLine(VersionTracking.GetStatus());

        Assert.Equal(currentBuild, VersionTracking.CurrentBuild);
        Assert.Equal("20", VersionTracking.PreviousBuild);
        Assert.Equal("10", VersionTracking.FirstInstalledBuild);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
        Assert.True(VersionTracking.IsFirstLaunchForCurrentBuild);

        VersionTracking.InitVersionTracking();

        Assert.Equal(currentBuild, VersionTracking.CurrentBuild);
        Assert.Equal("20", VersionTracking.PreviousBuild);
        Assert.Equal("10", VersionTracking.FirstInstalledBuild);
        Assert.False(VersionTracking.IsFirstLaunchEver);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentVersion);
        Assert.False(VersionTracking.IsFirstLaunchForCurrentBuild);
    }
}

file static class Extensions
{
    internal static void Track(this IVersionTracking i)
    {
    }

    internal static void InitVersionTracking(this IVersionTracking i)
    {
        i.GetType()
            .GetMethod("InitVersionTracking", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(i, null);
    }

    internal static string GetStatus(this IVersionTracking i)
    {
        var r = i.GetType()
            .GetMethod("GetStatus", BindingFlags.Public | BindingFlags.Instance)
            ?.Invoke(i, null)
            ?.ToString();
        return r!;
    }
}