using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;

public sealed record class AnalysisLogContainerModel
{
    [JsonPropertyName("logs")]
    public required List<AnalysisLogModel> Logs { get; set; }
}
