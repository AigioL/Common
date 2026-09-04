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
using System.Text;

namespace SAM.API.Wrappers;

public sealed class SteamUtils005 : NativeWrapper<ISteamUtils005>
{
    #region GetConnectedUniverse

    public unsafe int GetConnectedUniverse()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, int>)Functions.GetConnectedUniverse)(ObjectAddress);
        return __retVal;
    }

    #endregion

    #region GetIPCountry

    public unsafe string GetIPCountry()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte*>)Functions.GetIPCountry)(ObjectAddress);
        var strU8 = NativeStrings.PointerToSpan(__retVal);
        return Encoding.UTF8.GetString(strU8);
    }

    #endregion

    #region GetImageSize

    public unsafe bool GetImageSize(int index, out int width, out int height)
    {
        fixed (int* __width_native = &width)
        fixed (int* __height_native = &height)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, int*, int*, sbyte>)Functions.GetImageSize)(ObjectAddress, index, __width_native, __height_native);
            return __retVal != 0;
        }
    }

    #endregion

    #region GetImageRGBA

    public unsafe bool GetImageRGBA(int index, ReadOnlySpan<byte> data)
    {
        fixed (byte* __data_native = data)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, byte*, int, sbyte>)Functions.GetImageSize)(ObjectAddress, index, __data_native, data.Length);
            return __retVal != 0;
        }
    }

    #endregion

    #region GetAppID

    public unsafe uint GetAppId()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, uint>)Functions.GetAppID)(ObjectAddress);
        return __retVal;
    }

    #endregion
}