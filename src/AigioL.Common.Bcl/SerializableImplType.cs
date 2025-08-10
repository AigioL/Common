namespace System;

/// <summary>
/// 序列化程式实现种类
/// </summary>
public enum SerializableImplType : sbyte
{
    /// <summary>
    /// System.Text.Json
    /// <para>仅用于 .Net Core 3+ / Web，因 Emoji 字符被转义</para>
    /// <para>https://github.com/dotnet/corefx/tree/v3.1.5/src/System.Text.Json</para>
    /// <para>https://github.com/dotnet/runtime/tree/v5.0.0-preview.6.20305.6/src/libraries/System.Text.Json</para>
    /// </summary>
    SystemTextJson = 3,

    /// <summary>
    /// MemoryPack is Zero encoding extreme performance binary serializer for C# and Unity.
    /// <para>https://github.com/Cysharp/MemoryPack</para>
    /// </summary>
    MemoryPack = 4,
}
