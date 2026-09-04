using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SAM.API.Interfaces;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ISteamInventory002
{
    public nint GetResultStatus;
    public nint GetResultItems;
    public nint GetResultTimestamp;
    public nint CheckResultSteamID;
    public nint DestroyResult;
    public nint GetAllItems;
    public nint GetItemsByID;
    public nint SerializeResult;
    public nint DeserializeResult;
    public nint GenerateItems;
    public nint GrantPromoItems;
    public nint AddPromoItem;
    public nint AddPromoItems;
    public nint ConsumeItem;
    public nint ExchangeItems;
    public nint TransferItemQuantity;
    public nint SendItemDropHeartbeat;
    public nint TriggerItemDrop;
    public nint TradeItems;
    public nint LoadItemDefinitions;
    public nint GetItemDefinitionIDs;
    public nint GetItemDefinitionProperty;
    public nint GetNumItemsWithPrices;
    public nint DTorISteamInventory;
}
