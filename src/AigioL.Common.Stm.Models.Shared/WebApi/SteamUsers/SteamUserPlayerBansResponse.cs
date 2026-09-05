namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record class SteamUserPlayerBansResponse
{
    public SteamUserPlayerBansPlayer[] Players { get; set; } = [];

    /// <summary>
    /// <see cref="SteamUserPlayerBansResponse"/> 的 JSON 示例值
    /// </summary>
    public static ReadOnlySpan<byte> ExampleValue =>
"""
{
    "players": [
        {
            "SteamId": "71234567890123456",
            "CommunityBanned": false,
            "VACBanned": false,
            "NumberOfVACBans": 0,
            "DaysSinceLastBan": 0,
            "NumberOfGameBans": 0,
            "EconomyBan": "none"
        }
    ]
}
"""u8;
}
