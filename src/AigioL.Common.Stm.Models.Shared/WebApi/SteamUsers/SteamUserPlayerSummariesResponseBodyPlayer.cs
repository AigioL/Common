using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record class SteamUserPlayerSummariesResponseBodyPlayer
{
    [JsonPropertyName("steamid")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong SteamId { get; set; }

    [JsonPropertyName("communityvisibilitystate")]
    public int CommunityVisibilityState { get; set; }

    [JsonPropertyName("profilestate")]
    public int ProfileState { get; set; }

    [JsonPropertyName("personaname")]
    public string? PersonaName { get; set; }

    [JsonPropertyName("profileurl")]
    public string? ProfileUrl { get; set; }

    public string? Avatar { get; set; }

    [JsonPropertyName("avatarmedium")]
    public string? AvatarMedium { get; set; }

    [JsonPropertyName("avatarfull")]
    public string? AvatarFull { get; set; }

    [JsonPropertyName("avatarhash")]
    public string? AvatarHash { get; set; }

    [JsonPropertyName("personastate")]
    public int PersonaState { get; set; }

    [JsonPropertyName("realname")]
    public string? RealName { get; set; }

    [JsonPropertyName("primaryclanid")]
    public string? PrimaryClanId { get; set; }

    [JsonPropertyName("timecreated")]
    [JsonConverter(typeof(UnixTimeSecondsToDateTimeOffsetConverter))]
    public DateTimeOffset TimeCreated { get; set; }

    [JsonPropertyName("personastateflags")]
    public int PersonaStateFlags { get; set; }
}
