using AigioL.Common.AspNetCore.AppCenter;
using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Models;
using MemoryPack;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.Models;

public static partial class ApiRspExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteAsync(
        this ApiRsp value,
        SerializableImplType serializableImplType,
        HttpResponse response,
        CancellationToken cancellationToken = default)
        => value.WriteAsync(
            serializableImplType,
            response,
            MSMinimalApisJsonSerializerContext.Default.ApiRsp,
            cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteAsync(
        this ApiRsp value,
        HttpResponse response,
        CancellationToken cancellationToken = default)
        => value.WriteAsync(
            response,
            MSMinimalApisJsonSerializerContext.Default.ApiRsp,
            cancellationToken);

    public static async Task WriteAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>(
        this TValue value,
        SerializableImplType serializableImplType,
        HttpResponse response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
        where TValue : ApiRsp
    {
        switch (serializableImplType)
        {
            case SerializableImplType.MemoryPack:
                {
                    // https://github.com/Cysharp/MemoryPack/blob/1.21.4/src/MemoryPack.AspNetCoreMvcFormatter/MemoryPackOutputFormatter.cs#L55
                    var writer = response.BodyWriter;
                    response.Headers.ContentType = MediaTypeNames.MemoryPack;
                    MemoryPackSerializer.Serialize(typeof(TValue), writer, value);
                    await writer.FlushAsync(cancellationToken);
                }
                break;
            case SerializableImplType.SystemTextJson:
            default:
                {
                    await response.WriteAsJsonAsync(value,
                        jsonTypeInfo,
                        cancellationToken: cancellationToken);
                }
                break;
        }
    }

    public static Task WriteAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TValue>(
        this TValue value,
        HttpResponse response,
        JsonTypeInfo<TValue> jsonTypeInfo,
        CancellationToken cancellationToken = default)
        where TValue : ApiRsp
    {
        if (!MSMinimalApis.TryParse(response.HttpContext.Request.Headers.Accept, out var serializableImplType))
        {
            serializableImplType = SerializableImplType.SystemTextJson;
        }
        return value.WriteAsync(serializableImplType, response, jsonTypeInfo, cancellationToken);
    }
}
