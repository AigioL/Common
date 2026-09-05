using AigioL.Common.Stm.Models.Converters;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Econs.SteamEconomies;

public sealed partial record class AssetClassInfoResponseResult
{
    [JsonConverter(typeof(NullableLenientNumberBooleanJsonConverter))]
    public bool? Success { get; set; }

    public string? Error { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Pairs { get; set; }

    public ImmutableDictionary<ulong, AssetClassInfoResponseResultValue> GetPairs()
    {
        if (Pairs == null || Pairs.Count == 0)
        {
            return [];
        }
        else
        {
            Dictionary<ulong, AssetClassInfoResponseResultValue> r = new(Pairs.Count);
            foreach (var it in Pairs)
            {
                if (ulong.TryParse(it.Key, out var num))
                {
                    var value = it.Value.Deserialize(SteamJsonSerializerContext.Default.AssetClassInfoResponseResultValue);
                    if (value != null)
                    {
                        r[num] = value;
                    }
                }
            }
            return r.ToImmutableDictionary();
        }
    }
}