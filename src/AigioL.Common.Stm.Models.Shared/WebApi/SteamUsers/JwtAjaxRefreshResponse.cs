using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record class JwtAjaxRefreshResponse
{
    [JsonConverter(typeof(NullableLenientNumberBooleanJsonConverter))]
    public bool? Success { get; set; }

    [JsonPropertyName("login_url")]
    public string? LoginUrl { get; set; }

    [JsonPropertyName("steamID")]
    [JsonConverter(typeof(NullableUInt64ToStringJsonConverter))]
    public ulong? SteamId { get; set; }

    public string? Nonce { get; set; }

    public string? Redir { get; set; }

    public string? Auth { get; set; }

    public int? Error { get; set; } // 直接 POST 请求返回此值为 8

    public Dictionary<string, string?>? GetLoginSetTokenRequest(string redir = "/my")
    {
        if (string.IsNullOrWhiteSpace(Nonce) ||
            string.IsNullOrWhiteSpace(Auth) ||
            !SteamId.HasValue ||
            SteamId.Value == default)
        {
            return null;
        }
        return new()
        {
            { "steamID", SteamId.Value.ToString() },
            { "nonce", Nonce },
            { "redir", redir },
            { "auth", Auth },
        };
    }
}
