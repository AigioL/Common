using System.Net;
using System.Text.Json.Serialization;

namespace AigioL.Common.Net.NameResolution.Models;

internal sealed class DohJsonResponseModel
{
    /// <inheritdoc cref="DnsResponseCode"/>
    [JsonPropertyName("Status")]
    public DnsResponseCode Status { get; set; }

    [JsonPropertyName("Answer")]
    public List<DohJsonResponseAnswerModel>? Answer { get; set; }
}