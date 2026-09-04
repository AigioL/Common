using SAM.API.Interfaces;
using SAM.API.Types;

namespace SAM.API.Wrappers;

public sealed class SteamInventory002 : NativeWrapper<ISteamInventory002>
{
    #region GetAllItems

    public unsafe bool GetAllItems(ref nint pResultHandle)
    {
        fixed (nint* __pResultHandle_native = &pResultHandle)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, nint*, sbyte>)Functions.TriggerItemDrop)(ObjectAddress, __pResultHandle_native);
            return __retVal != 0;
        }
    }

    #endregion

    #region GetResultItems

    public unsafe bool GetResultItems(nint pResultHandle, ref SteamInventoryResult pOutItemsArray, ref uint punOutItemsArraySize)
    {
        fixed (SteamInventoryResult* __pOutItemsArray_native = &pOutItemsArray)
        fixed (uint* __punOutItemsArraySize_native = &punOutItemsArraySize)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, SteamInventoryResult*, uint*, sbyte>)Functions.GetResultItems)(ObjectAddress, pResultHandle, __pOutItemsArray_native, __punOutItemsArraySize_native);
            return __retVal != 0;
        }
    }

    #endregion

    #region GetNumItemsWithPrices

    public unsafe bool GetNumItemsWithPrices()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte>)Functions.GetNumItemsWithPrices)(ObjectAddress);
        return __retVal != 0;
    }

    #endregion

    #region DestroyResult

    public unsafe bool DestroyResult(nint resultHandle)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.DestroyResult)(ObjectAddress, resultHandle);
        return __retVal != 0;
    }

    #endregion

    #region GetItemDefinitionIDs

    public unsafe bool GetItemDefinitionIDs(ref uint pItemDefIDs, ref uint punItemDefIDsArraySize)
    {
        fixed (uint* __pItemDefIDs_native = &pItemDefIDs)
        fixed (uint* __punItemDefIDsArraySize_native = &punItemDefIDsArraySize)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, uint*, uint*, sbyte>)Functions.GetItemDefinitionIDs)(ObjectAddress, __pItemDefIDs_native, __punItemDefIDsArraySize_native);
            return __retVal != 0;
        }
    }

    #endregion

    #region TriggerItemDrop

    public unsafe bool TriggerItemDrop(ref nint pResultHandle, uint dropListDefinition)
    {
        fixed (nint* __pResultHandle_native = &pResultHandle)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, nint*, uint, sbyte>)Functions.TriggerItemDrop)(ObjectAddress, __pResultHandle_native, dropListDefinition);
            return __retVal != 0;
        }
    }

    #endregion
}
