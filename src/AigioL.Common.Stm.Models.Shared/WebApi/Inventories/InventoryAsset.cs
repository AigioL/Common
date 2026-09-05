using AigioL.Common.Stm.Models.Converters;
using System.Text.Json.Serialization;

namespace AigioL.Common.Stm.Models.WebApi.Inventories;

public sealed partial record class InventoryAsset
{
    public int AppId { get; set; }

    public int ContextId { get; set; }

    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong AssetId { get; set; }

    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong ClassId { get; set; }

    [JsonConverter(typeof(UInt64ToStringJsonConverter))]
    public ulong InstanceId { get; set; }

    public int Amount { get; set; }
}