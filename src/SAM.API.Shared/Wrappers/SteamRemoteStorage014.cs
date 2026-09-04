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
using SAM.API.Types;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace SAM.API.Wrappers;

public sealed class SteamRemoteStorage012 : NativeWrapper<ISteamRemoteStorage014>
{
    #region FileWrite

    public unsafe bool FileWrite(string pchFile, ReadOnlySpan<byte> pvData)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            fixed (byte* __pvData_native = pvData)
            {
                sbyte __retVal;
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, byte*, int, sbyte>)Functions.FileWrite)(ObjectAddress, handle.AddrOfPinnedObject(), __pvData_native, pvData.Length);
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
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, byte*, int, sbyte>)Functions.FileWrite)(ObjectAddress, (nint)p, __pvData_native, pvData.Length);
                    }
                }
                return __retVal != 0;
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

    #region FileRead

    public unsafe int FileRead(string pchFile, ReadOnlySpan<byte> pvData)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            fixed (byte* __pvData_native = pvData)
            {
                int __retVal;
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, byte*, int, int>)Functions.FileRead)(ObjectAddress, handle.AddrOfPinnedObject(), __pvData_native, pvData.Length);
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
                        __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, byte*, int, int>)Functions.FileRead)(ObjectAddress, (nint)p, __pvData_native, pvData.Length);
                    }
                }
                return __retVal;
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

    #region FileForget

    public unsafe bool FileForget(string pchFile)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FileForget)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FileForget)(ObjectAddress, (nint)p);
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

    #region FileDelete

    public unsafe bool FileDelete(string pchFile)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FileDelete)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FileDelete)(ObjectAddress, (nint)p);
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

    #region SetSyncPlatforms

    public unsafe bool SetSyncPlatforms(string pchFile, ERemoteStoragePlatform eRemoteStoragePlatform)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, ERemoteStoragePlatform, sbyte>)Functions.SetSyncPlatforms)(ObjectAddress, handle.AddrOfPinnedObject(), eRemoteStoragePlatform);
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, ERemoteStoragePlatform, sbyte>)Functions.SetSyncPlatforms)(ObjectAddress, (nint)p, eRemoteStoragePlatform);
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

    #region FileExists

    public unsafe bool FileExists(string pchFile)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FileExists)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FileExists)(ObjectAddress, (nint)p);
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

    #region FilePersisted

    public unsafe bool FilePersisted(string pchFile)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            sbyte __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FilePersisted)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, sbyte>)Functions.FilePersisted)(ObjectAddress, (nint)p);
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

    #region GetFileSize

    public unsafe int GetFileSize(string pchFile)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            int __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int>)Functions.GetFileSize)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, int>)Functions.GetFileSize)(ObjectAddress, (nint)p);
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

    #region GetFileTimestamp

    public unsafe long GetFileTimestamp(string pchFile)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            long __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, long>)Functions.GetFileTimestamp)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, long>)Functions.GetFileTimestamp)(ObjectAddress, (nint)p);
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

    #region GetSyncPlatforms

    public unsafe ERemoteStoragePlatform GetSyncPlatforms(string pchFile)
    {
        int expectedByteCount = Encoding.UTF8.GetMaxByteCount(pchFile.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);
        try
        {
            var len = Encoding.UTF8.GetBytes(pchFile, bytes);
            bytes[len..].Clear();
            ERemoteStoragePlatform __retVal;
            if (isArrayPool)
            {
                var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                try
                {
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, ERemoteStoragePlatform>)Functions.GetSyncPlatforms)(ObjectAddress, handle.AddrOfPinnedObject());
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
                    __retVal = ((delegate* unmanaged[Thiscall]<nint, nint, ERemoteStoragePlatform>)Functions.GetSyncPlatforms)(ObjectAddress, (nint)p);
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

    #region GetFileCount

    public unsafe int GetFileCount()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, int>)Functions.GetFileCount)(ObjectAddress);
        return __retVal;
    }

    #endregion

    #region GetFileNameAndSize

    public unsafe string GetFileNameAndSize(int iFile, out int pnFileSizeInBytes)
    {
        sbyte* __retVal;
        fixed (int* __pnFileSizeInBytes_native = &pnFileSizeInBytes)
        {
            __retVal = ((delegate* unmanaged[Thiscall]<nint, int, int*, sbyte*>)Functions.GetFileNameAndSize)(ObjectAddress, iFile, __pnFileSizeInBytes_native);
        }
        var strU8 = NativeStrings.PointerToSpan(__retVal);
        return Encoding.UTF8.GetString(strU8);
    }

    #endregion

    #region GetQuota

    public unsafe bool GetQuota(out ulong pnTotalBytes, out ulong puAvailableBytes)
    {
        fixed (ulong* __pnTotalBytes_native = &pnTotalBytes)
        fixed (ulong* __puAvailableBytes_native = &puAvailableBytes)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, ulong*, ulong*, sbyte>)Functions.GetQuota)(ObjectAddress, __pnTotalBytes_native, __puAvailableBytes_native);
            return __retVal != 0;
        }
    }

    #endregion

    #region IsCloudEnabledForAccount


    public unsafe bool IsCloudEnabledForAccount()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte>)Functions.IsCloudEnabledForAccount)(ObjectAddress);
        return __retVal != 0;
    }

    #endregion

    #region IsCloudEnabledForApp

    public unsafe bool IsCloudEnabledForApp()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, sbyte>)Functions.IsCloudEnabledForApp)(ObjectAddress);
        return __retVal != 0;
    }

    #endregion

    #region SetCloudEnabledForApp

    public unsafe void SetCloudEnabledForApp(bool bEnabled)
    {
        ((delegate* unmanaged[Thiscall]<nint, sbyte, void>)Functions.SetCloudEnabledForApp)(ObjectAddress, bEnabled ? (sbyte)1 : (sbyte)0);
    }

    #endregion

}