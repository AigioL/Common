using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;

public sealed record class AnalysisEventLogModel : AnalysisLogWithPropertiesModel
{
    public const string JsonIdentifier = "event";

    /// <summary>
    /// Gets or sets unique identifier for this event.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets name of the event.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
