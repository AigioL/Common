using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;

public sealed record class AnalysisStartServiceLogModel : AnalysisLogModel
{
    public const string JsonIdentifier = "startService";

    /// <summary>
    /// Gets or sets the list of services of the MobileCenter Start API
    /// call.
    /// </summary>
    [JsonPropertyName("services")]
    public List<string>? Services { get; set; }
}
