using System.Text.Json.Serialization;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Models.AppCenter;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AnalysisEventLogModel), typeDiscriminator: AnalysisEventLogModel.JsonIdentifier)]
[JsonDerivedType(typeof(AnalysisPageLogModel), typeDiscriminator: AnalysisPageLogModel.JsonIdentifier)]
[JsonDerivedType(typeof(AnalysisStartSessionLogModel), typeDiscriminator: AnalysisStartSessionLogModel.JsonIdentifier)]
[JsonDerivedType(typeof(AnalysisStartServiceLogModel), typeDiscriminator: AnalysisStartServiceLogModel.JsonIdentifier)]
public abstract record class AnalysisLogModel
{
    /// <summary>
    /// Gets or sets log timestamp, example: '2017-03-13T18:05:42Z'.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets when tracking an analytics session, logs can be part
    /// of the session by specifying this identifier.
    /// This attribute is optional, a missing value means the session
    /// tracking is disabled (like when using only error reporting
    /// feature).
    /// Concrete types like StartSessionLog or PageLog are always part of a
    /// session and always include this identifier.
    /// </summary>
    [JsonPropertyName("sid")]
    public Guid? Sid { get; set; }

    /// <summary>
    /// Gets or sets optional string used for associating logs with users.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("device")]
    public AnalysisDeviceModel? Device { get; set; }

    /// <summary>
    /// Gets or sets optional string used for specify data residency region.
    /// </summary>
    [JsonPropertyName("dataResidencyRegion")]
    public string? DataResidencyRegion { get; set; }
}
