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

    static byte[] ConvertEndian<[DynamicallyAccessedMembers(memberTypes)] T>(
        byte[] data)
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
                Array.Reverse(data, offset, length);
            }
        }

        return data;
    }

    static void ConvertEndian2<[DynamicallyAccessedMembers(memberTypes)] T>(
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

    public static T GetStruct<[DynamicallyAccessedMembers(memberTypes)] T>(byte[] data)
        where T : struct, IEndian
    {
        return GetStruct<T>(data, 0, data.Length);
    }

    public static T GetStruct<[DynamicallyAccessedMembers(memberTypes)] T>(byte[] data, int offset, int length)
        where T : struct, IEndian
    {
        return GetStruct<T>(data.AsSpan(offset, length));
    }

    public static T GetStruct<[DynamicallyAccessedMembers(memberTypes)] T>(ReadOnlySpan<byte> data)
        where T : struct, IEndian
    {
        var buffer = ArrayPool<byte>.Shared.Rent(data.Length);
        try
        {
            data.CopyTo(buffer);
            ConvertEndian2<T>(buffer.AsSpan(0, data.Length));
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [Obsolete("use Write<T>(T, Span<byte>) instead", true)]
    public static byte[] GetBytes<[DynamicallyAccessedMembers(memberTypes)] T>(T obj)
        where T : struct, IEndian
    {
        var size = Marshal.SizeOf(obj);
        var data = GC.AllocateUninitializedArray<byte>(size);

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
            return ConvertEndian<T>(data);
        }
        finally
        {
            handle.Free();
        }
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
        ConvertEndian2<T>(data);
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
    }
}
