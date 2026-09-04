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

public sealed class SteamApps001 : NativeWrapper<ISteamApps001>
{
    #region GetAppData

    static unsafe string GetAppData(
        nint funcPtr,
        nint self,
        uint appId,
        nint key)
    {
        const int valueLength = 1024;
        byte[] versionArr = ArrayPool<byte>.Shared.Rent(valueLength);
        var handle = GCHandle.Alloc(versionArr, GCHandleType.Pinned);
        try
        {
            var valuePointer = handle.AddrOfPinnedObject();
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, uint, nint, nint, int, int>)funcPtr)(self, appId, key, valuePointer, valueLength);
            if (__retVal == 0)
            {
                return string.Empty;
            }
            var index = versionArr.IndexOf((byte)0);
            return Encoding.UTF8.GetString(versionArr, 0, index);
        }
        finally
        {
            handle.Free();
            ArrayPool<byte>.Shared.Return(versionArr);
        }
    }

    public string GetAppData(uint appId, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return GetAppData(Functions.GetAppData,
                ObjectAddress,
                appId,
                default);
        }

        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(key.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);

        try
        {
            var len = Encoding.UTF8.GetBytes(key, bytes);
            bytes[len..].Clear();
            unsafe
            {
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        return GetAppData(Functions.GetAppData,
                            ObjectAddress,
                            appId,
                            handle.AddrOfPinnedObject());
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
                else
                {
                    fixed (byte* p = bytes)
                    {
                        return GetAppData(Functions.GetAppData,
                            ObjectAddress,
                            appId,
                            (nint)p);
                    }
                }
            }
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }

    #endregion
}