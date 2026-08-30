// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/runtime/blob/v11.0.0-preview.7.26381.103/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs

#if NET461 || NET452 || NET451 || NET45 || NET40 || NET35
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
using System.Runtime.CompilerServices;

namespace System.Numerics;

internal static class BitOperations
{

    /// <summary>
    /// Rotates the specified value left by the specified number of bits.
    /// Similar in behavior to the x86 instruction ROL.
    /// </summary>
    /// <param name="value">The value to rotate.</param>
    /// <param name="offset">The number of bits to rotate by.
    /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
    /// <returns>The rotated value.</returns>
    [MethodImpl(256)]
    public static uint RotateLeft(uint value, int offset)
        => (value << offset) | (value >> (32 - offset));

    /// <summary>
    /// Rotates the specified value right by the specified number of bits.
    /// Similar in behavior to the x86 instruction ROR.
    /// </summary>
    /// <param name="value">The value to rotate.</param>
    /// <param name="offset">The number of bits to rotate by.
    /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
    /// <returns>The rotated value.</returns>
    [MethodImpl(256)]
    public static uint RotateRight(uint value, int offset)
        => (value >> offset) | (value << (32 - offset));
}
#endif
