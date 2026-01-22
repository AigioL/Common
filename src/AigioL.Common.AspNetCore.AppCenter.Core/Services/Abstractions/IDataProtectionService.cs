using MemoryPack;
using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

/// <summary>
/// 数据保护服务接口
/// </summary>
public interface IDataProtectionService
{
    ValueTask<byte[]> ProtectAsync(byte[] plainBytes, CancellationToken cancellationToken = default);

    ValueTask<byte[]> ProtectAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T t, CancellationToken cancellationToken = default)
    {
        var plainBytes = MemoryPackSerializer.Serialize(t);
        return ProtectAsync(plainBytes, cancellationToken);
    }

    ValueTask<byte[]> UnprotectAsync(byte[] protectedBytes, CancellationToken cancellationToken = default);

    async ValueTask<T?> UnprotectAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(byte[] protectedBytes, CancellationToken cancellationToken = default)
    {
        var plainBytes = await UnprotectAsync(protectedBytes, cancellationToken);
        var t = MemoryPackSerializer.Deserialize<T>(plainBytes);
        return t;
    }
}
