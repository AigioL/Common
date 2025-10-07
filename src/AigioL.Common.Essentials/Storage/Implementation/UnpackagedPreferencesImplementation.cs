using MemoryPack;
using MemoryPack.Compression;
using Microsoft.IO;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using PreferencesDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, byte[]>>;
using ShareNameDictionary = System.Collections.Concurrent.ConcurrentDictionary<string, byte[]>;

namespace AigioL.Common.Essentials.Storage.Implementation;

/// <summary>
/// https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/src/Preferences/Preferences.windows.cs#L148
/// </summary>
sealed class UnpackagedPreferencesImplementation : IPreferences
{
    readonly string appPreferencesPath;

    readonly PreferencesDictionary _preferences = new();

    internal UnpackagedPreferencesImplementation(string appDataDirectory)
    {
        appPreferencesPath = Path.Combine(appDataDirectory, "preferences.dbf");

        Load();

        _preferences.GetOrAdd(string.Empty, _ => new ShareNameDictionary());
    }

    public bool ContainsKey(string key, string? sharedName = default)
    {
        if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
        {
            return inner.ContainsKey(key);
        }

        return false;
    }

    public void Remove(string key, string? sharedName = default)
    {
        if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
        {
            inner.TryRemove(key, out _);
            Save();
        }
    }

    public void Clear(string? sharedName = default)
    {
        if (_preferences.TryGetValue(CleanSharedName(sharedName), out var prefs))
        {
            prefs.Clear();
            Save();
        }
    }

    public void Set<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? value, string? sharedName = default)
    {
        //Preferences.CheckIsSupportedType<T>();

        var prefs = _preferences.GetOrAdd(CleanSharedName(sharedName), _ => new ShareNameDictionary());

        if (value is null)
        {
            prefs.TryRemove(key, out _);
        }
        else
        {
            var bytes = IBinarySerialize.SerializeCore(value);
            prefs[key] = bytes;
        }

        Save();
    }

    public T? Get<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(string key, T? defaultValue, string? sharedName = default)
    {
        if (_preferences.TryGetValue(CleanSharedName(sharedName), out var inner))
        {
            if (inner.TryGetValue(key, out var value) && value is not null)
            {
                var obj = IBinarySerialize.DeserializeCore(value, defaultValue);
                return obj;
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v9.0.8/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    internal const int StackallocByteThreshold = 256;

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v10.0.0-rc.1.25451.107/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L92
    /// </summary>
    internal const int MaximumFormatDateTimeLength = 27;    // StandardFormat 'O', e.g. 2017-06-12T05:30:45.7680000
    internal const int MaximumFormatDateTimeOffsetLength = 33;  // StandardFormat 'O', e.g. 2017-06-12T05:30:45.7680000-07:00

    void Load()
    {
        if (!File.Exists(appPreferencesPath))
        {
            return;
        }

        byte[]? buffer = null;
        try
        {
            using var fs = File.OpenRead(appPreferencesPath);
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

            var readPreferences = MemoryPackSerializer.Deserialize<PreferencesDictionary>(decompressedBuffer);

            if (readPreferences != null)
            {
                _preferences.Clear();
                foreach (var pair in readPreferences)
                    _preferences.TryAdd(pair.Key, pair.Value);
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
            var dir = Path.GetDirectoryName(appPreferencesPath);
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
                MemoryPackSerializer.Serialize(typeof(PreferencesDictionary), compressor, _preferences);
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
            using var fs = File.Create(appPreferencesPath);
            buffer.CopyTo(fs);
            fs.Flush();
            fs.SetLength(fs.Position);
        }
    }

    internal static bool IsInvalidConcurrrentCollectionOperation(MemoryPackSerializationException e)
    {
        // https://github.com/Cysharp/MemoryPack/blob/1.21.4/src/MemoryPack.Core/MemoryPackSerializationException.cs#L103
        if (e.Message == "ConcurrentCollection is Added/Removed in serializing, however serialize concurrent collection is not thread-safe.")
        {
            return true;
        }
        return false;
    }


    internal static readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager = new();

    static string CleanSharedName(string? sharedName) =>
        string.IsNullOrEmpty(sharedName) ? string.Empty : sharedName;
}
