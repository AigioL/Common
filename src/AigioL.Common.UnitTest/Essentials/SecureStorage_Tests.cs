using AigioL.Common.Essentials.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace AigioL.Common.UnitTest.Essentials;

public sealed partial class SecureStorage_Tests : BaseUnitTest
{
    ISecureStorage SecureStorage { get; }
}

partial class SecureStorage_Tests // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/test/UnitTests/SecureStorage_Tests.cs
{
    [Fact]
    public async Task SecureStorage_LoadAsync()
    {
        await SecureStorage.GetAsync<string>("key");
    }

    [Fact]
    public async Task SecureStorage_SaveAsync()
    {
        await SecureStorage.SetAsync("key", "data");
    }
}

[Category("SecureStorage")]
[Collection("UsesPreferences")]
partial class SecureStorage_Tests // https://github.com/dotnet/maui/blob/10.0.0-rc.1.25424.2/src/Essentials/test/DeviceTests/Tests/SecureStorage_Tests.cs
{
    public SecureStorage_Tests()
    {
        SecureStorage = Program.Services.GetRequiredService<ISecureStorage>();
        SecureStorage.RemoveAll();
    }

    [Theory
#if MACCATALYST
            (Skip = "Need to configure entitlements.")
#endif
    ]
    [InlineData("test.txt", "data")]
    [InlineData("noextension", "data2")]
    [InlineData("funny*&$%@!._/\\chars", "data3")]
    [InlineData("test.txt2", "data2")]
    [InlineData("noextension2", "data22")]
    [InlineData("funny*&$%@!._/\\chars2", "data32")]
    public async Task Saves_And_Loads(string key, string data)
    {
#if __IOS__
        // Try the new platform specific api
        await SecureStorage.SetAsync(key, data, Security.SecAccessible.AfterFirstUnlock);

        var b = await SecureStorage.GetAsync(key);

        Assert.Equal(data, b);
#endif
        await SecureStorage.SetAsync(key, data);

        var c = await SecureStorage.GetAsync<string>(key);

        Assert.Equal(data, c);
    }

    [Theory
#if MACCATALYST
            (Skip = "Need to configure entitlements.")
#endif
    ]
    [InlineData("test.txt", "data1", "data2")]
    public async Task Saves_Same_Key_Twice(string key, string data1, string data2)
    {
        Console.WriteLine(Environment.ProcessPath);

        await SecureStorage.SetAsync(key, data1);
        await SecureStorage.SetAsync(key, data2);

        var c = await SecureStorage.GetAsync<string>(key);

        Assert.Equal(data2, c);
    }

#if __ANDROID__
    [Theory]
    [InlineData("test.txt", "data")]
    public async Task Fix_Corrupt_Data(string key, string data)
    {
        // this operation is only available on API level 23+ devices
        if (!OperatingSystem.IsAndroidVersionAtLeast(23))
            return;

        // set a valid key
        await SecureStorage.SetAsync(key, data);

        // simulate corrupt the key
        var corruptData = "A2PfJSNdEDjM+422tpu7FqFcVQQbO3ti/DvnDnIqrq9CFwaBi6NdXYcicjvMW6nF7X/Clpto5xerM41U1H4qtWJDO0Ijc5QNTHGZl9tDSbXJ6yDCDDnEDryj2uTa8DiHoNcNX68QtcV3at4kkJKXXAwZXSC88a73/xDdh1u5gUdCeXJzVc5vOY6QpAGUH0bjR5NHrqEQNNGDdquFGN9n2ZJPsEK6C9fx0QwCIL+uldpAYSWrpmUIr+/0X7Y0mJpN84ldygEVxHLBuVrzB4Bbu5XGLUN/0Sr2plWcKm7XhM6wp3JRW6Eae2ozys42p1YLeM0HXWrhTqP6FRPkS6mOtw==";
        var all = PreferencesImplementation.GetSharedPreferences(SecureStorageImplementation.Alias).All;
        Preferences.Set(all.Keys.First(x => !x.StartsWith("_")), corruptData, SecureStorageImplementation.Alias);

        var c = await SecureStorage.GetAsync<string>(key);
        Assert.Null(c);

        // try to reset and get again
        await SecureStorage.SetAsync(key, data);
        c = await SecureStorage.GetAsync<string>(key);
        Assert.Equal(data, c);
    }

    [Fact]
    public void Set_Get_Wait_MultipleTimes()
    {
        for (int i = 0; i < 100; i++)
        {
            var set = SecureStorage.SetAsync(i.ToString(), i.ToString());
            set.Wait();

            var get = SecureStorage.GetAsync<string>(i.ToString());
            get.Wait();

            Assert.Equal(i.ToString(), get.Result);
        }
    }
#endif

    [Fact
#if MACCATALYST
            (Skip = "Need to configure entitlements.")
#endif
    ]
    public async Task Non_Existent_Key_Returns_Null()
    {
        var v = await SecureStorage.GetAsync<string>("THIS_KEY_SHOULD_NOT_EXIST");
        Assert.Null(v);
    }

    [Theory
#if MACCATALYST
            (Skip = "Need to configure entitlements.")
#endif
    ]
    [InlineData("KEY_TO_REMOVE1")]
    [InlineData("KEY_TO_REMOVE2")]
    public async Task Remove_Key(string key)
    {
        await SecureStorage.SetAsync(key, "Irrelevant Data");

        var result = SecureStorage.Remove(key);
        Assert.True(result);

        var v = await SecureStorage.GetAsync<string>(key);
        Assert.Null(v);
    }

    [Theory
#if MACCATALYST
            (Skip = "Need to configure entitlements.")
#endif
    ]
    [InlineData("KEYS_TO_REMOVEA1", "KEYS_TO_REMOVEA2")]
    [InlineData("KEYS_TO_REMOVEB1", "KEYS_TO_REMOVEB2")]
    public async Task Remove_All_Keys(string key1, string key2)
    {
        string[] keys = new[] { key1, key2 };

        // Set a couple keys
        foreach (var key in keys)
            await SecureStorage.SetAsync(key, "Irrelevant Data");

        // Remove them all
        SecureStorage.RemoveAll();

        // Make sure they are all removed
        foreach (var key in keys)
        {
            var result = await SecureStorage.GetAsync<string>(key);
            Assert.Null(result);
        }
    }

    //#if __ANDROID__
    [Fact]
    public async Task Asymmetric_to_Symmetric_API_Upgrade()
    {
        Console.WriteLine(Environment.ProcessPath);

        var key = "asym_to_sym_upgrade";
        var expected = "this is the value";

        SecureStorage.RemoveAll();

        await SecureStorage.SetAsync(key, expected);

        var v = await SecureStorage.GetAsync<string>(key);

        SecureStorage.RemoveAll();

        Assert.Equal(expected, v);
    }
    //#endif

    [Fact
#if MACCATALYST
        (Skip = "Need to configure entitlements.")
#endif
    //#if WINDOWS
    //        (Skip = "IOException on unpackaged: The process cannot access the file...")
    //#endif
    ]
    public async Task Set_Get_Async_MultipleTimes()
    {
        const int count = 100;
        Console.WriteLine(Environment.ProcessPath);

        await Parallel.ForEachAsync(Enumerable.Range(0, count), async (i, _) =>
            await SecureStorage.SetAsync(i.ToString(), i.ToString())
        );

        for (int i = 0; i < count; i++)
        {
            var v = await SecureStorage.GetAsync<string>(i.ToString());
            Assert.Equal(i.ToString(), v);
        }
    }

    [Fact
#if MACCATALYST
        (Skip = "Need to configure entitlements.")
#endif
    //#if WINDOWS
    //        (Skip = "IOException on unpackaged: The process cannot access the file...")
    //#endif
    ]
    public async Task Set_Get_Sync_MultipleTimes()
    {
        const int count = 100;
        Console.WriteLine(Environment.ProcessPath);

        for (int i = 0; i < count; i++)
        {
            var key = $"key3{i}";
            var value = $"value3{i}";
            await SecureStorage.SetAsync(key, value);

            var v = await SecureStorage.GetAsync<string>(key);
            Assert.Equal(value, v);
        }
    }

    [Fact
#if MACCATALYST
        (Skip = "Need to configure entitlements.")
#endif
    //#if WINDOWS
    //        (Skip = "IOException on unpackaged: The process cannot access the file...")
    //#endif
    ]
    public async Task Set_Get_Remove_Async_MultipleTimes()
    {
        Console.WriteLine(Environment.ProcessPath);

        await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, _) =>
        {
            var key = $"key{i}";
            var value = $"value{i}";
            await SecureStorage.SetAsync(key, value);

            var fetched = await SecureStorage.GetAsync<string>(key);
            Assert.Equal(value, fetched);
            SecureStorage.Remove(key);
            fetched = await SecureStorage.GetAsync<string>(key);
            Assert.Null(fetched);
        });
    }

    [Fact
#if MACCATALYST
        (Skip = "Need to configure entitlements.")
#endif
    //#if WINDOWS
    //        (Skip = "IOException on unpackaged: The process cannot access the file...")
    //#endif
    ]
    public async Task Set_Get_Remove_Sync_MultipleTimes()
    {
        Console.WriteLine(Environment.ProcessPath);

        for (int i = 0; i < 100; i++)
        {
            var key = $"key2{i}";
            var value = $"value2{i}";
            await SecureStorage.SetAsync(key, value);

            var fetched = await SecureStorage.GetAsync<string>(key);
            Assert.Equal(value, fetched);
            SecureStorage.Remove(key);
            fetched = await SecureStorage.GetAsync<string>(key);
            Assert.Null(fetched);
        }
    }
}