/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using SAM.API.Interfaces;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace SAM.API.Wrappers;

public sealed class SteamApps008 : NativeWrapper<ISteamApps008>
{
    #region IsSubscribed

    public unsafe bool IsSubscribedApp(uint appid)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, uint, sbyte>)Functions.IsSubscribedApp)(ObjectAddress, appid);
        return __retVal != 0;
    }

    #endregion

    #region GetCurrentGameLanguage

    public unsafe string GetCurrentGameLanguage()
    {
        var strPointer = ((delegate* unmanaged[Thiscall]<nint, sbyte*>)Functions.GetCurrentGameLanguage)(ObjectAddress);
        var strU8 = NativeStrings.PointerToSpan(strPointer);
        return Encoding.UTF8.GetString(strU8);
    }

    #endregion

    #region GetAvailableGameLanguages

    public unsafe string GetAvailableGameLanguages()
    {
        var strPointer = ((delegate* unmanaged[Thiscall]<nint, sbyte*>)Functions.GetAvailableGameLanguages)(ObjectAddress);
        var strU8 = NativeStrings.PointerToSpan(strPointer);
        return Encoding.UTF8.GetString(strU8);
    }

    #endregion

    #region IsDlcInstalled

    public unsafe bool IsDlcInstalled(uint appid)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, uint, sbyte>)Functions.IsDlcInstalled)(ObjectAddress, appid);
        return __retVal != 0;
    }

    #endregion

    #region IsAppInstalled

    public unsafe bool IsAppInstalled(uint appid)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, uint, sbyte>)Functions.IsAppInstalled)(ObjectAddress, appid);
        return __retVal != 0;
    }

    #endregion

    #region IsSubscribedFromFamilySharing

    public unsafe bool IsSubscribedFromFamilySharing()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte>)Functions.IsSubscribedFromFamilySharing)(ObjectAddress);
        return __retVal != 0;
    }

    #endregion

    #region IsSubscribedFromFreeWeekend

    public unsafe bool IsSubscribedFromFreeWeekend()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte>)Functions.IsSubscribedFromFreeWeekend)(ObjectAddress);
        return __retVal != 0;
    }

    #endregion

    #region GetAppInstallDir

    public unsafe string GetAppInstallDir(uint appid)
    {
        byte[] array = ArrayPool<byte>.Shared.Rent(Helpers.MemoryBufferSize);
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            var mempchFolder = handle.AddrOfPinnedObject();
            ((delegate* unmanaged[Thiscall]<nint, uint, nint, uint, nint>)Functions.GetAppInstallDir)(ObjectAddress, appid, mempchFolder, Helpers.MemoryBufferSize);
            var pchFolder = Helpers.MemoryToSpan(array.AsSpan(0, Helpers.MemoryBufferSize));
            return Encoding.UTF8.GetString(pchFolder);
        }
        finally
        {
            handle.Free();
            ArrayPool<byte>.Shared.Return(array);
        }
    }

    #endregion
}