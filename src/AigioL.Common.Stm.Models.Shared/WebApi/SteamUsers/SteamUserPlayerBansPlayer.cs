using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record class SteamUserPlayerBansPlayer
{
    [JsonPropertyName("SteamId")]
    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong SteamId { get; set; }

    /// <summary>
    /// 布尔值，用于指示玩家是否被禁止进入社区
    /// </summary>
    [JsonPropertyName("CommunityBanned")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool CommunityBanned { get; set; }

    /// <summary>
    /// 布尔值，用于指示玩家是否存在 VAC 禁令记录
    /// </summary>
    [JsonPropertyName("VACBanned")]
    [JsonConverter(typeof(LenientNumberBooleanJsonConverter))]
    public bool VACBanned { get; set; }

    [JsonPropertyName("NumberOfVACBans")]
    public int NumberOfVACBans { get; set; }

    [JsonPropertyName("DaysSinceLastBan")]
    public int DaysSinceLastBan { get; set; }

    /// <summary>
    /// 游戏中的禁用次数
    /// </summary>
    [JsonPropertyName("NumberOfGameBans")]
    public int NumberOfGameBans { get; set; }

    /// <summary>
    /// 包含玩家在经济系统中封禁状态的字符串。若玩家无封禁记录，则显示为“none”；若处于观察期，则显示为“probation”，依此类推
    /// </summary>
    [JsonPropertyName("EconomyBan")]
    public string? EconomyBan { get; set; }
}