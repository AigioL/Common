namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record class SteamUserPlayerSummariesResponse
{
    public SteamUserPlayerSummariesResponseBody Response { get; set; } = new();

    /// <summary>
    /// <see cref="SteamUserPlayerSummariesResponse"/> 的 JSON 示例值
    /// </summary>
    public static ReadOnlySpan<byte> ExampleValue =>
"""
{
	"response": {
		"players": {
			"player": [
				{
					"steamid": "71234567890123456",
					"communityvisibilitystate": 3,
					"profilestate": 1,
					"personaname": "ABCDEFG",
					"profileurl": "https://steamcommunity.com/id/ABCDEFG/",
					"avatar": "https://avatars.steamstatic.com/1234567890123456789012345678901234567890.jpg",
					"avatarmedium": "https://avatars.steamstatic.com/1234567890123456789012345678901234567890.jpg",
					"avatarfull": "https://avatars.steamstatic.com/1234567890123456789012345678901234567890.jpg",
					"avatarhash": "1234567890123456789012345678901234567890",
					"personastate": 0,
					"realname": "ABCDEFG",
					"primaryclanid": "123456789012345678",
					"timecreated": 1636751513,
					"personastateflags": 0
				},
				{
					"steamid": "71234567890123456",
					"communityvisibilitystate": 3,
					"profilestate": 1,
					"personaname": "ABCDEFG",
					"profileurl": "https://steamcommunity.com/id/ABCDEFG/",
					"avatar": "https://avatars.steamstatic.com/1234567890123456789012345678901234567890.jpg",
					"avatarmedium": "https://avatars.steamstatic.com/1234567890123456789012345678901234567890.jpg",
					"avatarfull": "https://avatars.steamstatic.com/1234567890123456789012345678901234567890.jpg",
					"avatarhash": "1234567890123456789012345678901234567890",
					"personastate": 0,
					"realname": "ABCDEFG",
					"primaryclanid": "123456789012345678",
					"timecreated": 1636751513,
					"personastateflags": 0
				}
			]
		}
	}
}
"""u8;
}
