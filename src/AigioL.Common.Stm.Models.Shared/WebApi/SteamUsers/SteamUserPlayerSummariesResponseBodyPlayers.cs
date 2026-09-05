namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record class SteamUserPlayerSummariesResponseBodyPlayers
{
    public SteamUserPlayerSummariesResponseBodyPlayer[] Player { get; set; } = [];
}
