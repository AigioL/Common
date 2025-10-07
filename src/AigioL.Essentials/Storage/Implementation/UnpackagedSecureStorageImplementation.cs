using MemoryPack;
using MemoryPack.Compression;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using static AigioL.Essentials.Storage.Implementation.UnpackagedPreferencesImplementation;
using SecureStorageDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, byte[]>;

namespace AigioL.Essentials.Storage.Implementation;

/// <summary>
/// https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/SecureStorage/SecureStorage.windows.cs#L110
/// </summary>
sealed partial class UnpackagedSecureStorageImplementation : ISecureStorage
{
    readonly string appSecureStoragePath;
    readonly string alias;
    readonly SecureStorageDictionary _secureStorage = new();
    readonly IPreferences preferences;
    readonly bool isCurrentUserOrLocalMachine;

    internal UnpackagedSecureStorageImplementation(
        IPreferences preferences,
        string packageName,
        string appDataDirectory,
        bool isCurrentUserOrLocalMachine = false)
    {
        this.preferences = preferences;
        appSecureStoragePath = Path.Combine(appDataDirectory, "securestorage.dbf");
        alias = $"{packageName}.microsoft.maui.essentials.preferences"; // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/SecureStorage/SecureStorage.shared.cs#L207
        this.isCurrentUserOrLocalMachine = isCurrentUserOrLocalMachine;
#if WINDOWS
        lazyUseWinRTDataProtectionProvider = new(() => C5bd0b1b.UseWinRTDataProtectionProvider(preferences, alias), LazyThreadSafetyMode.ExecutionAndPublication);
#else
        lazyAes = new(() => C5bd0b1b.GetAes256(alias), LazyThreadSafetyMode.ExecutionAndPublication);
#endif

        Load();
    }

    void Load()
    {
        if (!File.Exists(appSecureStoragePath))
        {
            return;
        }

        byte[]? buffer = null;
        try
        {
            using var fs = File.OpenRead(appSecureStoragePath);
            var len = fs.Length;
            if (len == 0 || len > int.MaxValue)
            {
                return;
            }
            var len32 = unchecked((int)len);
            Span<byte> bytes = len32 <= StackallocByteThreshold ?
                stackalloc byte[StackallocByteThreshold] :
                ((buffer = ArrayPool<byte>.Shared.Rent(len32)).AsSpan(0, len32));
            fs.ReadExactly(bytes);
            fs.Dispose();

            using var decompressor = new BrotliDecompressor();
            var decompressedBuffer = decompressor.Decompress(bytes);

            var readPreferences = MemoryPackSerializer.Deserialize<SecureStorageDictionary>(decompressedBuffer);

            if (readPreferences != null)
            {
                _secureStorage.Clear();
                foreach (var pair in readPreferences)
                    _secureStorage.TryAdd(pair.Key, pair.Value);
            }
        }
        catch
        {
            // if deserialization fails proceed with empty settings
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    readonly Lock lockSave = new();

    void Save()
    {
        lock (lockSave)
        {
            var dir = Path.GetDirectoryName(appSecureStoragePath);
            if (dir != null)
            {
                Directory.CreateDirectory(dir);
            }

            // 先序列化，将数据保存到内存流中
            using var compressor = new BrotliCompressor();
            using var buffer = recyclableMemoryStreamManager.GetStream();
            try
            {
                // https://github.com/Cysharp/MemoryPack/blob/1.21.4/src/MemoryPack.AspNetCoreMvcFormatter/MemoryPackOutputFormatter.cs#L54
                MemoryPackSerializer.Serialize(typeof(SecureStorageDictionary), compressor, _secureStorage, MemoryPackSerializerOptions.Utf8);
                compressor.CopyTo(buffer);
            }
            catch (MemoryPackSerializationException e)
            {
                if (IsInvalidConcurrrentCollectionOperation(e))
                {
                    // 忽略序列化完成后的 count 检查，因为并发集合在序列化过程中可能会被修改，在保存操作时加锁即可
                }
                else
                {
                    throw;
                }
            }
            buffer.Flush();
            buffer.Position = 0;

            // 再从内存流中将数据复制到文件上
            using var fs = File.Create(appSecureStoragePath);
            buffer.CopyTo(fs);
            fs.Flush();
            fs.SetLength(fs.Position);
        }
    }

    public Task<byte[]?> GetAsync(string key)
    {
        _secureStorage.TryGetValue(key, out var value);
        if (value == null)
        {
            return Task.FromResult<byte[]?>(null);
        }
        return UnprotectAsync(value)!;
    }

    public async Task<T?> GetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? defaultValue = default)
    {
        var bytes = await GetAsync(key);
        if (bytes != null)
        {
            if (typeof(T) == typeof(byte[]))
            {
                return Unsafe.As<byte[], T>(ref bytes);
            }
            else
            {
                try
                {
                    return IBinarySerialize.DeserializeCore<T>(bytes, defaultValue);
                }
                finally
                {
                    // 清空内存数组，数据安全
                    Array.Clear(bytes);
                }
            }
        }
        return defaultValue;
    }

    public async Task SetAsync(string key, byte[]? value)
    {
        if (value is null)
        {
            _secureStorage.TryRemove(key, out _);
        }
        else
        {
            var protectValue = await ProtectAsync(value);
            _secureStorage[key] = protectValue;
        }
        Save();
    }

    public async Task SetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? value)
    {
        byte[]? buffer = null;
        try
        {
            var bytes = value is null ? null :
                typeof(T) == typeof(byte[]) ?
                Unsafe.As<T, byte[]>(ref value) :
                buffer = IBinarySerialize.SerializeCore(value);
            await SetAsync(key, bytes);
        }
        finally
        {
            if (buffer != null)
            {
                // 清空内存数组，数据安全
                Array.Clear(buffer);
            }
        }
    }

    public bool Remove(string key)
    {
        var result = _secureStorage.TryRemove(key, out _);
        Save();
        return result;
    }

    public void RemoveAll()
    {
        _secureStorage.Clear();
        Save();
    }

    Task<byte[]> UnprotectAsync(byte[] data)
    {
        if (data.Length != 0)
        {
#if WINDOWS
            if (UseWinRTDataProtectionProvider())
            {
                return UnprotectByWinRTAsync(data);
            }
            else
            {
                return UnprotectByCrypt32Async(data);
            }
#else
            var aes = GetAes();
            var bytes = aes.DecryptCbc(data, aes.IV);
            return Task.FromResult(bytes);
#endif
        }
        return Task.FromResult(data);
    }

    Task<byte[]> ProtectAsync(byte[] data)
    {
        if (data.Length != 0)
        {
#if WINDOWS
            if (UseWinRTDataProtectionProvider())
            {
                return ProtectByWinRTAsync(data);
            }
            else
            {
                return ProtectByCrypt32Async(data);
            }
#else
            var aes = GetAes();
            var bytes = aes.EncryptCbc(data, aes.IV);
            return Task.FromResult(bytes);
#endif
        }
        return Task.FromResult(data);
    }
}

#if !WINDOWS
partial class UnpackagedSecureStorageImplementation
{
    Lazy<Aes> lazyAes;

    Aes GetAes()
    {
        if (disposedValue || lazyAes == null)
        {
            throw new ObjectDisposedException(nameof(lazyAes));
        }
        return lazyAes.Value;
    }
}
#endif

partial class UnpackagedSecureStorageImplementation : IDisposable
{
    bool disposedValue;

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
#if !WINDOWS
                if (lazyAes.IsValueCreated)
                {
                    lazyAes.Value.Dispose();
                }
#endif
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
#if !WINDOWS
            lazyAes = null!;
#endif
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}



file static partial class C5bd0b1b
{
#if WINDOWS
    const string KeyUseCrypt32 = "USE_CRYPTPROTECTDATA";

    internal static bool UseWinRTDataProtectionProvider(IPreferences preferences, string sharedName)
    {
        var useCrypt32 = preferences.Get(KeyUseCrypt32, false, sharedName);
        if (useCrypt32)
        {
            return false;
        }

        var isWin10AtLeast = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240);
        if (isWin10AtLeast)
        {
            return true;
        }
        else
        {
            // 低于 Windows 10 的系统，使用 Crypt32 API，并存储该偏好
            preferences.Set(KeyUseCrypt32, true, sharedName);
            return false;
        }
    }
#else
    internal static Aes GetAes256(string key)
    {
        byte[]? buffer = null;
        try
        {
            var len = Encoding.UTF8.GetMaxByteCount(key.Length);
            Span<byte> bytes = len <= StackallocByteThreshold ?
                stackalloc byte[StackallocByteThreshold] :
                (buffer = ArrayPool<byte>.Shared.Rent(len)).AsSpan(0, len);
            var bytesWritten = Encoding.UTF8.GetBytes(key, bytes);
            bytes = bytes[..bytesWritten];
            Span<byte> sha384 = stackalloc byte[SHA384.HashSizeInBytes];
            SHA384.HashData(bytes, sha384);
            var aes = Aes.Create();
            aes.KeySize = 256;
            aes.IV = sha384[..16].ToArray();
            aes.Key = sha384.Slice(16, 32).ToArray();
            return aes;
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
#endif
}