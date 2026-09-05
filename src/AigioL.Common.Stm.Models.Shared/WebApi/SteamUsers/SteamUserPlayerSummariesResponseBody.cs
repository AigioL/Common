namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record class SteamUserPlayerSummariesResponseBody
{
    public SteamUserPlayerSummariesResponseBodyPlayers Players { get; set; } = new();
}
