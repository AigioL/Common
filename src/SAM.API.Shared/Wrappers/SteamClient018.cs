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

namespace SAM.API.Wrappers;

public sealed class SteamClient018 : NativeWrapper<ISteamClient018>
{
    #region CreateSteamPipe

    public unsafe int CreateSteamPipe()
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, int>)Functions.CreateSteamPipe)(ObjectAddress);
        return __retVal;
    }

    #endregion

    #region ReleaseSteamPipe

    public unsafe bool ReleaseSteamPipe(int pipe)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, sbyte>)Functions.ReleaseSteamPipe)(ObjectAddress, pipe);
        return __retVal != 0;
    }

    #endregion

    #region CreateLocalUser

    public unsafe int CreateLocalUser(ref int pipe, Types.AccountType type)
    {
        fixed (int* __pipe_native = &pipe)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int*, Types.AccountType, int>)Functions.CreateLocalUser)(ObjectAddress, __pipe_native, type);
            return __retVal;
        }
    }

    #endregion

    #region ConnectToGlobalUser

    public unsafe int ConnectToGlobalUser(int pipe)
    {
        var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, int>)Functions.ConnectToGlobalUser)(ObjectAddress, pipe);
        return __retVal;
    }

    #endregion

    #region ReleaseUser

    public unsafe void ReleaseUser(int pipe, int user)
    {
        ((delegate* unmanaged[Thiscall]<nint, int, int, void>)Functions.ReleaseUser)(ObjectAddress, pipe, user);
    }

    #endregion

    #region SetLocalIPBinding

    public unsafe void SetLocalIPBinding(uint host, ushort port)
    {
        ((delegate* unmanaged[Thiscall]<nint, uint, ushort, void>)Functions.SetLocalIPBinding)(ObjectAddress, host, port);
    }

    #endregion

    #region GetISteamUser

    private unsafe TClass GetISteamUser<TClass>(int user, int pipe, ReadOnlySpan<byte> version)
        where TClass : INativeWrapper, new()
    {
        fixed (byte* versionPtr = version)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, int, byte*, nint>)Functions.GetISteamUser)(ObjectAddress, user, pipe, versionPtr);
            var result = new TClass();
            result.SetupFunctions(__retVal);
            return result;
        }
    }

    #endregion

    #region GetSteamUser017

    public SteamUser017 GetSteamUser017(int user, int pipe)
    {
        return GetISteamUser<SteamUser017>(user, pipe, "SteamUser017"u8);
    }

    #endregion

    #region GetISteamUserStats

    private unsafe TClass GetISteamUserStats<TClass>(int user, int pipe, ReadOnlySpan<byte> version)
        where TClass : INativeWrapper, new()
    {
        fixed (byte* versionPtr = version)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, int, byte*, nint>)Functions.GetISteamUserStats)(ObjectAddress, user, pipe, versionPtr);
            var result = new TClass();
            result.SetupFunctions(__retVal);
            return result;
        }
    }

    #endregion

    #region GetSteamUserStats011

    public SteamUserStats011 GetSteamUserStats011(int user, int pipe)
    {
        return GetISteamUserStats<SteamUserStats011>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION011"u8);
    }

    #region GetSteamUserStats013

    public SteamUserStats013 GetSteamUserStats013(int user, int pipe)
    {
        return GetISteamUserStats<SteamUserStats013>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION013"u8);
    }

    #endregion

    #endregion

    #region GetISteamUtils

    public unsafe TClass GetISteamUtils<TClass>(int pipe, ReadOnlySpan<byte> version)
        where TClass : INativeWrapper, new()
    {
        fixed (byte* versionPtr = version)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, byte*, nint>)Functions.GetISteamUtils)(ObjectAddress, pipe, versionPtr);
            var result = new TClass();
            result.SetupFunctions(__retVal);
            return result;
        }
    }

    #endregion

    #region GetSteamUtils009

    public SteamUtils007 GetSteamUtils007(int pipe)
    {
        return GetISteamUtils<SteamUtils007>(pipe, "SteamUtils007"u8);
    }

    #endregion

    #region GetISteamApps

    private unsafe TClass GetISteamApps<TClass>(int user, int pipe, ReadOnlySpan<byte> version)
        where TClass : INativeWrapper, new()
    {
        fixed (byte* versionPtr = version)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, int, byte*, nint>)Functions.GetISteamApps)(ObjectAddress, user, pipe, versionPtr);
            var result = new TClass();
            result.SetupFunctions(__retVal);
            return result;
        }
    }

    #endregion

    #region GetSteamApps001

    public SteamApps001 GetSteamApps001(int user, int pipe)
    {
        return GetISteamApps<SteamApps001>(user, pipe, "STEAMAPPS_INTERFACE_VERSION001"u8);
    }

    #endregion

    #region GetSteamApps008

    public SteamApps008 GetSteamApps008(int user, int pipe)
    {
        return GetISteamApps<SteamApps008>(user, pipe, "STEAMAPPS_INTERFACE_VERSION008"u8);
    }

    #endregion

    #region GetISteamRemoteStorage

    private unsafe TClass GetISteamRemoteStorage<TClass>(int user, int pipe, ReadOnlySpan<byte> version)
        where TClass : INativeWrapper, new()
    {
        fixed (byte* versionPtr = version)
        {
            var __retVal = ((delegate* unmanaged[Thiscall]<nint, int, int, byte*, nint>)Functions.GetISteamRemoteStorage)(ObjectAddress, user, pipe, versionPtr);
            var result = new TClass();
            result.SetupFunctions(__retVal);
            return result;
        }
    }

    #endregion

    #region GetSteamRemoteStorage012

    public SteamRemoteStorage012 GetSteamRemoteStorage012(int user, int pipe)
    {
        return GetISteamRemoteStorage<SteamRemoteStorage012>(user, pipe, "STEAMREMOTESTORAGE_INTERFACE_VERSION012"u8);
    }

    #endregion

    #region GetSteamInventory

    public SteamInventory002 GetSteamInventory002(int user, int pipe)
    {
        return GetISteamApps<SteamInventory002>(user, pipe, "STEAMINVENTORY_INTERFACE_V002"u8);
    }

    #endregion
}