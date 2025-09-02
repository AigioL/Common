using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;

public abstract record class AnalysisLogWithPropertiesModel : AnalysisLogModel
{
    /// <summary>
    /// Gets or sets additional key/value pair parameters.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, string>? Properties { get; set; }
}