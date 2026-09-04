/* Copyright (c) 2024 Rick (rick 'at' gibbed 'dot' us)
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

public sealed class SteamUserStats013 : NativeWrapper<ISteamUserStats013>
{
    #region GetStatValue (int)

    public unsafe bool GetStatValue(string name, out int value)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            fixed (int* __value_native = &value)
            {
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int*, sbyte>)Functions.GetStatInteger)(ObjectAddress, handle.AddrOfPinnedObject(), __value_native);
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
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int*, sbyte>)Functions.GetStatInteger)(ObjectAddress, (nint)p, __value_native);
                    }
                }
            }
            return __retVal != 0;
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

    #region GetStatValue (float)

    public unsafe bool GetStatValue(string name, out float value)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            fixed (float* __value_native = &value)
            {
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, float*, sbyte>)Functions.GetStatFloat)(ObjectAddress, handle.AddrOfPinnedObject(), __value_native);
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
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, float*, sbyte>)Functions.GetStatFloat)(ObjectAddress, (nint)p, __value_native);
                    }
                }
            }
            return __retVal != 0;
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

    #region SetStatValue (int)

    public unsafe bool SetStatValue(string name, int value)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int, sbyte>)Functions.SetStatInteger)(ObjectAddress, handle.AddrOfPinnedObject(), value);
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int, sbyte>)Functions.SetStatInteger)(ObjectAddress, (nint)p, value);
                }
            }
            return __retVal != 0;
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

    #region SetStatValue (float)

    public unsafe bool SetStatValue(string name, float value)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, float, sbyte>)Functions.SetStatFloat)(ObjectAddress, handle.AddrOfPinnedObject(), value);
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, float, sbyte>)Functions.SetStatFloat)(ObjectAddress, (nint)p, value);
                }
            }
            return __retVal != 0;
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

    #region GetAchievement

    public bool GetAchievementState(string name, out bool isAchieved)
    {
        var __retVal = GetAchievementStateCore(name, out var isAchieved1);
        isAchieved = isAchieved1 != 0;
        return __retVal;
    }

    private unsafe bool GetAchievementStateCore(string name, out sbyte isAchieved)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            fixed (sbyte* __value_native = &isAchieved)
            {
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte*, sbyte>)Functions.GetAchievement)(ObjectAddress, handle.AddrOfPinnedObject(), __value_native);
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
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte*, sbyte>)Functions.GetAchievement)(ObjectAddress, (nint)p, __value_native);
                    }
                }
            }
            return __retVal != 0;
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

    #region SetAchievementState

    public unsafe bool SetAchievement(string name, bool state)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            nint funcPtr = state ? Functions.SetAchievement : Functions.ClearAchievement;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)funcPtr)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)funcPtr)(ObjectAddress, (nint)p);
                }
            }
            return __retVal != 0;
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

    #region GetAchievementAndUnlockTime

    public bool GetAchievementAndUnlockTime(string name, out bool isAchieved, out long unlockTime)
    {
        var __retVal = GetAchievementAndUnlockTimeCore(name, out var isAchieved1, out unlockTime);
        isAchieved = isAchieved1 != 0;
        return __retVal;
    }

    private unsafe bool GetAchievementAndUnlockTimeCore(string name, out sbyte isAchieved, out long unlockTime)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            fixed (sbyte* __value_native = &isAchieved)
            fixed (long* __unlockTime_native = &unlockTime)
            {
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte*, long*, sbyte>)Functions.GetAchievementAndUnlockTime)(ObjectAddress, handle.AddrOfPinnedObject(), __value_native, __unlockTime_native);
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
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte*, long*, sbyte>)Functions.GetAchievementAndUnlockTime)(ObjectAddress, (nint)p, __value_native, __unlockTime_native);
                    }
                }
            }
            return __retVal != 0;
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

    #region StoreStats

    public unsafe bool StoreStats()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte>)Functions.StoreStats)(ObjectAddress);
        return __retVal != 0;
    }

    #endregion

    #region GetAchievementIcon

    public unsafe int GetAchievementIcon(string name)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            int __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int>)Functions.GetAchievementIcon)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int>)Functions.GetAchievementIcon)(ObjectAddress, (nint)p);
                }
            }
            return __retVal;
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

    #region GetAchievementDisplayAttribute

    public unsafe string GetAchievementDisplayAttribute(string name, string key)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(name.Length) + 1;
        int expectedByteCount2 = Encoding.UTF8.GetMaxByteCount(key.Length) + 1;

        byte[]? array = null, array2 = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        bool isArrayPool2 = expectedByteCount2 > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        Span<byte> bytes2 = !isArrayPool2 ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array2 = ArrayPool<byte>.Shared.Rent(expectedByteCount2)).AsSpan(0, expectedByteCount2);
        try
        {
            var len = Encoding.UTF8.GetBytes(name, bytes);
            bytes[len..].Clear();
            var len2 = Encoding.UTF8.GetBytes(key, bytes2);
            bytes2[len2..].Clear();

            sbyte* __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    if (isArrayPool2)
                    {
                        var handle2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
                        try
                        {
                            __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, nint, sbyte*>)Functions.GetAchievementDisplayAttribute)(ObjectAddress, handle.AddrOfPinnedObject(), handle2.AddrOfPinnedObject());
                        }
                        finally
                        {
                            handle2.Free();
                        }
                    }
                    else
                    {
                        fixed (byte* p2 = bytes2)
                        {
                            __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, nint, sbyte*>)Functions.GetAchievementDisplayAttribute)(ObjectAddress, handle.AddrOfPinnedObject(), (nint)p2);
                        }
                    }
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
                    if (isArrayPool2)
                    {
                        var handle2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
                        try
                        {
                            __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, nint, sbyte*>)Functions.GetAchievementDisplayAttribute)(ObjectAddress, (nint)p, handle2.AddrOfPinnedObject());
                        }
                        finally
                        {
                            handle2.Free();
                        }
                    }
                    else
                    {
                        fixed (byte* p2 = bytes2)
                        {
                            __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, nint, sbyte*>)Functions.GetAchievementDisplayAttribute)(ObjectAddress, (nint)p, (nint)p2);
                        }
                    }
                }
            }
            var strU8 = NativeStrings.PointerToSpan(__retVal);
            return Encoding.UTF8.GetString(strU8);
        }
        finally
        {
            if (array is not null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
            if (array2 is not null)
            {
                ArrayPool<byte>.Shared.Return(array2);
            }
        }
    }

    #endregion

    #region RequestUserStats

    public unsafe CallHandle RequestUserStats(ulong steamIdUser)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, ulong, CallHandle>)Functions.RequestUserStats)(ObjectAddress, steamIdUser);
        return __retVal;
    }

    #endregion

    #region ResetAllStats

    public unsafe bool ResetAllStats(bool achievementsToo)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte, sbyte>)Functions.ResetAllStats)(ObjectAddress, (achievementsToo ? (sbyte)1 : (sbyte)0));
        return __retVal != 0;
    }

    #endregion
}