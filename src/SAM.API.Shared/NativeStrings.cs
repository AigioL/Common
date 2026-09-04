/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace SAM.API;

internal static class NativeStrings
{
    /// <summary>
    /// Converts a zero-terminated pointer to a span by scanning until <c>\0</c> without an explicit length limit.
    /// </summary>
    /// <remarks>
    /// This method is only suitable when no maximum length is available.
    /// If a maximum length is known, prefer creating a bounded span and using <see cref="MemoryExtensions.IndexOf{T}(ReadOnlySpan{T}, T)"/>
    /// (for example, <c>IndexOf((byte)0)</c>) to benefit from SIMD-optimized search.
    /// </remarks>
    public static unsafe ReadOnlySpan<byte> PointerToSpan(sbyte* bytes)
    {
        if (bytes == null)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        int running = 0;

        var b = bytes;
        if (*b == 0)
        {
            return ReadOnlySpan<byte>.Empty;
        }

        while ((*b++) != 0)
        {
            running++;
        }

        return new ReadOnlySpan<byte>(bytes, running);
    }
}