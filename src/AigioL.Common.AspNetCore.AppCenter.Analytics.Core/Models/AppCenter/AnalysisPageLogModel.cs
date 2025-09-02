using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;

public sealed record class AnalysisPageLogModel : AnalysisLogWithPropertiesModel
{
    public const string JsonIdentifier = "page";

    /// <summary>
    /// Gets or sets name of the page.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
