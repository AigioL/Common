#if WINDOWS
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;

namespace AigioL.Common.Essentials.Storage.Implementation;

partial class UnpackagedSecureStorageImplementation
{
    readonly Lazy<bool> lazyUseWinRTDataProtectionProvider;
    readonly DataProtectionScope dataProtectionScope;
    readonly Lazy<byte[]> lazyEntropy;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool UseWinRTDataProtectionProvider() => lazyUseWinRTDataProtectionProvider.Value;

    async Task<byte[]> UnprotectByWinRTAsync(byte[] data)
    {
        // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/SecureStorage/SecureStorage.windows.cs#L36
        var provider = GetDataProtectionProvider();
        var buffer = await provider.UnprotectAsync(data.AsBuffer());
        return buffer.ToArray();
    }

    Task<byte[]> UnprotectByCrypt32Async(byte[] data)
    {
        var optionalEntropy = lazyEntropy.Value;
        try
        {
            var buffer = ProtectedData.Unprotect(data, optionalEntropy, dataProtectionScope);
            return Task.FromResult(buffer);
        }
        finally
        {
            Array.Clear(optionalEntropy);
        }
    }

    // LOCAL=user and LOCAL=machine do not require enterprise auth capability
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    DataProtectionProvider GetDataProtectionProvider() => new(isCurrentUserOrLocalMachine ? "LOCAL=user" : "LOCAL=machine");

    async Task<byte[]> ProtectByWinRTAsync(byte[] data)
    {
        // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/SecureStorage/SecureStorage.windows.cs#L36
        var provider = GetDataProtectionProvider();
        var buffer = await provider.ProtectAsync(data.AsBuffer());
        return buffer.ToArray();
    }

    Task<byte[]> ProtectByCrypt32Async(byte[] data)
    {
        var optionalEntropy = lazyEntropy.Value;
        try
        {
            var buffer = ProtectedData.Protect(data, optionalEntropy, dataProtectionScope);
            return Task.FromResult(buffer);
        }
        finally
        {
            Array.Clear(optionalEntropy);
        }
    }
}
#endif