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

using Microsoft.Win32;
using System.Buffers;
using System.Runtime.InteropServices;

namespace SAM.API;

public static partial class Steam
{
    private static nint _Handle = IntPtr.Zero;

    public static Func<string?>? GetInstallPathDelegate { private get; set; }

    public static string? GetInstallPath()
    {
        var @delegate = GetInstallPathDelegate;
        if (@delegate != null)
        {
            return @delegate();
        }
#if !WINDOWS
        if (OperatingSystem.IsWindows())
#endif
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null)?.ToString();
        }
#if !WINDOWS
        else
        {
            throw new PlatformNotSupportedException();
        }
#endif
    }

    private static delegate* unmanaged[Cdecl]<sbyte*, nint, nint> _CallCreateInterface;

    public static TClass? CreateInterface<TClass>(string version)
        where TClass : INativeWrapper, new()
    {
        ArgumentNullException.ThrowIfNull(version);

        int expectedByteCount = Helpers.Ansi.GetMaxByteCount(version.Length) + 1;

        byte[]? array = null;
        bool isArrayPool = expectedByteCount > Helpers.StackallocByteThreshold;
        Span<byte> bytes = !isArrayPool ?
            stackalloc byte[Helpers.StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(expectedByteCount)).AsSpan(0, expectedByteCount);

        nint address = default;
        try
        {
            var len = Helpers.Ansi.GetBytes(version, bytes);
            bytes[len..].Clear();
            unsafe
            {
                if (isArrayPool)
                {
                    var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                    try
                    {
                        address = _CallCreateInterface((sbyte*)handle.AddrOfPinnedObject(), default);
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
                        address = _CallCreateInterface((sbyte*)p, default);
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

        if (address == IntPtr.Zero)
        {
            return default;
        }

        var rez = new TClass();
        rez.SetupFunctions(address);
        return rez;
    }

    private static delegate* unmanaged[Cdecl]<int, Types.CallbackMessage*, int*, sbyte> _CallSteamBGetCallback;

    public static unsafe bool GetCallback(int pipe, out Types.CallbackMessage message, out int call)
    {
        ArgumentNullException.ThrowIfNull(_CallSteamBGetCallback);

        message = default;
        call = default;
        sbyte __retVal;
        // Pin - Pin data in preparation for calling the P/Invoke.
        fixed (int* __call_native = &call)
        fixed (Types.CallbackMessage* __message_native = &message)
        {
            __retVal = _CallSteamBGetCallback(pipe, __message_native, __call_native);
        }

        return __retVal != 0;
    }

    private static delegate* unmanaged[Cdecl]<int, sbyte> _CallSteamFreeLastCallback;

    public static unsafe bool FreeLastCallback(int pipe)
    {
        ArgumentNullException.ThrowIfNull(_CallSteamFreeLastCallback);

        var __retVal = _CallSteamFreeLastCallback(pipe);

        return __retVal != 0;
    }

    public static bool Load()
    {
        try
        {
            if (_Handle != IntPtr.Zero)
            {
                return true;
            }

            var path = GetInstallPath();
            if (path == null)
            {
                return false;
            }

#if !WINDOWS
            if (OperatingSystem.IsMacOS())
            {
                path = Path.Combine(path, "steamclient.dylib");
            }
            else if (OperatingSystem.IsWindows())
#endif
            {
                // C:\Program Files (x86)\Steam\steamclient64.dll
                path = Path.Combine(path,
                    Environment.Is64BitProcess ?
                        "steamclient64.dll" :
                        "steamclient.dll");
            }
#if !WINDOWS
            else if (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid())
            {
                // /home/{0}/.local/share/Steam/linux64/steamclient.so
                path = Path.Combine(path,
                    Environment.Is64BitProcess ?
                        "linux64" :
                        "linux32",
                    "steamclient.so");
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
#endif

            nint module = NativeLibrary.Load(path);

            if (module == IntPtr.Zero)
            {
                return false;
            }

            unsafe
            {
                _CallCreateInterface = (delegate* unmanaged[Cdecl]<sbyte*, nint, nint>)NativeLibrary.GetExport(module, "CreateInterface");
                if (_CallCreateInterface == null)
                {
                    return false;
                }

                _CallSteamBGetCallback = (delegate* unmanaged[Cdecl]<int, Types.CallbackMessage*, int*, sbyte>)NativeLibrary.GetExport(module, "Steam_BGetCallback");
                if (_CallSteamBGetCallback == null)
                {
                    return false;
                }

                _CallSteamFreeLastCallback = (delegate* unmanaged[Cdecl]<int, sbyte>)NativeLibrary.GetExport(module, "Steam_FreeLastCallback");
                if (_CallSteamFreeLastCallback == null)
                {
                    return false;
                }
            }

            _Handle = module;
            return true;
        }
        catch
        {
            return false;
        }
    }
}