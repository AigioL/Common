using Microsoft.IO;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;

public sealed partial record class AnalysisDeviceModel
{
    /// <summary>
    /// Gets or sets name of the SDK. Consists of the name of the SDK and
    /// the platform, e.g. "mobilecenter.ios", "hockeysdk.android".
    /// </summary>
    [JsonPropertyName("sdkName")]
    public string? SdkName { get; set; }

    /// <summary>
    /// Gets or sets version of the SDK in semver format, e.g. "1.2.0" or
    /// "0.12.3-alpha.1".
    /// </summary>
    [JsonPropertyName("sdkVersion")]
    public string? SdkVersion { get; set; }

    /// <summary>
    /// Gets or sets version of the wrapper SDK in semver format. When the
    /// SDK is embedding another base SDK (for example Xamarin.Android
    /// wraps Android), the Xamarin specific version is populated into this
    /// field while sdkVersion refers to the original Android SDK.
    /// </summary>
    [JsonPropertyName("wrapperSdkVersion")]
    public string? WrapperSdkVersion { get; set; }

    /// <summary>
    /// Gets or sets name of the wrapper SDK. Consists of the name of the
    /// SDK and the wrapper platform, e.g. "mobilecenter.xamarin",
    /// "hockeysdk.cordova".
    /// </summary>
    [JsonPropertyName("wrapperSdkName")]
    public string? WrapperSdkName { get; set; }

    /// <summary>
    /// Gets or sets device model (example: iPad2,3).
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets device manufacturer (example: HTC).
    /// </summary>
    [JsonPropertyName("oemName")]
    public string? OemName { get; set; }

    /// <summary>
    /// Gets or sets OS name (example: iOS). The following OS names are
    /// standardized (non-exclusive): Android, iOS, macOS, tvOS, Windows.
    /// </summary>
    [JsonPropertyName("osName")]
    public string? OsName { get; set; }

    /// <summary>
    /// Gets or sets OS version (example: 9.3.0).
    /// </summary>
    [JsonPropertyName("osVersion")]
    public string? OsVersion { get; set; }

    /// <summary>
    /// Gets or sets OS build code (example: LMY47X).
    /// </summary>
    [JsonPropertyName("osBuild")]
    public string? OsBuild { get; set; }

    /// <summary>
    /// Gets or sets API level when applicable like in Android (example:
    /// 15).
    /// </summary>
    [JsonPropertyName("osApiLevel")]
    public int? OsApiLevel { get; set; }

    /// <summary>
    /// Gets or sets language code (example: en-US).
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the offset in minutes from UTC for the device time
    /// zone, including daylight savings time.
    /// </summary>
    [JsonPropertyName("timeZoneOffset")]
    public int TimeZoneOffset { get; set; }

    /// <summary>
    /// Gets or sets screen size of the device in pixels (example:
    /// 640x480).
    /// </summary>
    [JsonPropertyName("screenSize")]
    public string? ScreenSize { get; set; }

    /// <summary>
    /// Gets or sets application version name, e.g. 1.1.0
    /// </summary>
    [JsonPropertyName("appVersion")]
    public string? AppVersion { get; set; }

    /// <summary>
    /// Gets or sets carrier name (for mobile devices).
    /// </summary>
    [JsonPropertyName("carrierName")]
    public string? CarrierName { get; set; }

    /// <summary>
    /// Gets or sets carrier country code (for mobile devices).
    /// </summary>
    [JsonPropertyName("carrierCountry")]
    public string? CarrierCountry { get; set; }

    /// <summary>
    /// Gets or sets the app's build number, e.g. 42.
    /// </summary>
    [JsonPropertyName("appBuild")]
    public string? AppBuild { get; set; }

    /// <summary>
    /// Gets or sets the bundle identifier, package identifier, or
    /// namespace, depending on what the individual plattforms use,  .e.g
    /// com.microsoft.example.
    /// </summary>
    [JsonPropertyName("appNamespace")]
    public string? AppNamespace { get; set; }

    /// <summary>
    /// Gets or sets label that is used to identify application code
    /// 'version' released via Live Update beacon running on device
    /// </summary>
    [JsonPropertyName("liveUpdateReleaseLabel")]
    public string? LiveUpdateReleaseLabel { get; set; }

    /// <summary>
    /// Gets or sets identifier of environment that current application
    /// release belongs to, deployment key then maps to environment like
    /// Production, Staging.
    /// </summary>
    [JsonPropertyName("liveUpdateDeploymentKey")]
    public string? LiveUpdateDeploymentKey { get; set; }

    /// <summary>
    /// Gets or sets hash of all files (ReactNative or Cordova) deployed to
    /// device via LiveUpdate beacon. Helps identify the Release version on
    /// device or need to download updates in future.
    /// </summary>
    [JsonPropertyName("liveUpdatePackageHash")]
    public string? LiveUpdatePackageHash { get; set; }

    /// <summary>
    /// Gets or sets version of the wrapper technology framework (Xamarin
    /// runtime version or ReactNative or Cordova etc...). See
    /// wrappersdkname to see if this version refers to Xamarin or
    /// ReactNative or other.
    /// </summary>
    [JsonPropertyName("wrapperRuntimeVersion")]
    public string? WrapperRuntimeVersion { get; set; }
}

partial record class AnalysisDeviceModel
{
    string GetHash256(Stream s)
    {
        var pos = s.Position;

        StreamWriter w = new(s, Encoding.UTF8);
        w.Write(SdkName);
        w.Write(':');
        w.Write(SdkVersion);
        w.Write(':');
        w.Write(WrapperSdkVersion);
        w.Write(':');
        w.Write(WrapperSdkName);
        w.Write(':');
        w.Write(Model);
        w.Write(':');
        w.Write(OemName);
        w.Write(':');
        w.Write(OsName);
        w.Write(':');
        w.Write(OsVersion);
        w.Write(':');
        w.Write(OsBuild);
        w.Write(':');
        if (OsApiLevel.HasValue)
        {
            w.Write(OsApiLevel.Value);
        }
        w.Write(':');
        w.Write(Locale);
        w.Write(':');
        w.Write(TimeZoneOffset);
        w.Write(':');
        w.Write(ScreenSize);
        w.Write(':');
        w.Write(AppVersion);
        w.Write(':');
        w.Write(CarrierName);
        w.Write(':');
        w.Write(CarrierCountry);
        w.Write(':');
        w.Write(AppBuild);
        w.Write(':');
        w.Write(AppNamespace);
        w.Write(':');
        w.Write(LiveUpdateReleaseLabel);
        w.Write(':');
        w.Write(LiveUpdateDeploymentKey);
        w.Write(':');
        w.Write(LiveUpdatePackageHash);
        w.Write(':');
        w.Write(WrapperRuntimeVersion);

        w.Flush();
        try
        {
            s.Position = pos;
        }
        catch
        {
        }

        Span<byte> bytes = stackalloc byte[SHA256.HashSizeInBits];
        SHA256.HashData(s, bytes);
        return Convert.ToHexString(bytes);
    }

    public string GetHash256()
    {
        using var s = _.m.GetStream();
        var result = GetHash256(s);
        return result;
    }

    [return: NotNullIfNotNull(nameof(m))]
    public static string? GetHash256(AnalysisDeviceModel? m)
    {
        if (m == null)
        {
            return null;
        }
        return m.GetHash256();
    }
}

file static class _
{
    internal static readonly RecyclableMemoryStreamManager m = new();
}
