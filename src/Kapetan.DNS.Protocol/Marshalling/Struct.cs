using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DNS.Protocol.Marshalling;

public static class StructHelper
{
    const DynamicallyAccessedMemberTypes memberTypes =
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors |
        DynamicallyAccessedMemberTypes.AllFields;

    static void ConvertEndian<[DynamicallyAccessedMembers(memberTypes)] T>(
        Span<byte> data)
        where T : struct, IEndian
    {
        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        var endianness = T.GetEndianness();
        foreach (FieldInfo field in fields)
        {
            int offset = Marshal.OffsetOf<T>(field.Name).ToInt32();
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            int length = Marshal.SizeOf(field.FieldType);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

            if (endianness == Endianness.Big && BitConverter.IsLittleEndian ||
                   endianness == Endianness.Little && !BitConverter.IsLittleEndian)
            {
                data.Slice(offset, length).Reverse();
            }
        }
    }

    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Text.Json/Common/JsonConstants.cs#L12
    /// </summary>
    public const int StackallocByteThreshold = 256;

    public static T GetStruct<[DynamicallyAccessedMembers(memberTypes)] T>(ReadOnlySpan<byte> data)
        where T : struct, IEndian
    {
        T result;
        byte[]? array = null;
        Span<byte> buffer = data.Length <= StackallocByteThreshold ?
            stackalloc byte[StackallocByteThreshold] :
            (array = ArrayPool<byte>.Shared.Rent(data.Length)).AsSpan(0, data.Length);

        try
        {
            data.CopyTo(buffer); // 复制到缓冲区
            ConvertEndian<T>(buffer); // 大小端调整
            result = MemoryMarshal.AsRef<T>(buffer); // 零拷贝
        }
        finally
        {
            if (array != null)
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
        return result;
    }

    public static ref T GetRefStruct<[DynamicallyAccessedMembers(memberTypes)] T>(ReadOnlySpan<byte> data, Span<byte> buffer)
        where T : struct, IEndian
    {
        buffer = buffer[..data.Length]; // byte 缓冲区

        data.CopyTo(buffer); // 写入数据
        ConvertEndian<T>(buffer); // 大小端调整

        return ref MemoryMarshal.AsRef<T>(buffer); // 零拷贝内存直接将字节范围重新解释结构体
    }

    public static unsafe void Write<[DynamicallyAccessedMembers(memberTypes)] T>(T obj, Span<byte> result)
        where T : struct, IEndian
    {
        var size = Marshal.SizeOf(obj);
        if (result.Length < size)
        {
            throw new ArgumentException("Result span is too small");
        }

        Span<byte> data = result[..size];
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
        ReadOnlySpan<byte> objPtr = new(&obj, size);
        objPtr.CopyTo(data);
        ConvertEndian<T>(data);
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
    }
}
