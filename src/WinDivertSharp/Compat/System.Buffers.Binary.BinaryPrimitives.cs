// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Private.CoreLib/src/System/Buffers/Binary/BinaryPrimitives.ReverseEndianness.cs

#if NET461 || NET452 || NET451 || NET45 || NET40 || NET35
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace System.Buffers.Binary;

static class BinaryPrimitives
{
    /// <summary>
    /// Reverses a primitive value by performing an endianness swap of the specified <see cref="ushort" /> value.
    /// </summary>
    /// <param name="value">The value to reverse.</param>
    /// <returns>The reversed value.</returns>
    [MethodImpl(256)]
    public static ushort ReverseEndianness(ushort value)
    {
        // Don't need to AND with 0xFF00 or 0x00FF since the final
        // cast back to ushort will clear out all bits above [ 15 .. 00 ].
        // This is normally implemented via "movzx eax, ax" on the return.
        // Alternatively, the compiler could elide the movzx instruction
        // entirely if it knows the caller is only going to access "ax"
        // instead of "eax" / "rax" when the function returns.

        return (ushort)((value >> 8) + (value << 8));
    }

    /// <summary>
    /// Reverses a primitive value by performing an endianness swap of the specified <see cref="uint" /> value.
    /// </summary>
    /// <param name="value">The value to reverse.</param>
    /// <returns>The reversed value.</returns>
    [MethodImpl(256)]
    public static uint ReverseEndianness(uint value)
    {
        // This takes advantage of the fact that the JIT can detect
        // ROL32 / ROR32 patterns and output the correct intrinsic.
        //
        // Input: value = [ ww xx yy zz ]
        //
        // First line generates : [ ww xx yy zz ]
        //                      & [ 00 FF 00 FF ]
        //                      = [ 00 xx 00 zz ]
        //             ROR32(8) = [ zz 00 xx 00 ]
        //
        // Second line generates: [ ww xx yy zz ]
        //                      & [ FF 00 FF 00 ]
        //                      = [ ww 00 yy 00 ]
        //             ROL32(8) = [ 00 yy 00 ww ]
        //
        //                (sum) = [ zz yy xx ww ]
        //
        // Testing shows that throughput increases if the AND
        // is performed before the ROL / ROR.

        return BitOperations.RotateRight(value & 0x00FF00FFu, 8) // xx zz
            + BitOperations.RotateLeft(value & 0xFF00FF00u, 8); // ww yy
    }
}
#endif