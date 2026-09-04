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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SAM.API;

public abstract class NativeWrapper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] TNativeFunctions> : INativeWrapper
    where TNativeFunctions : struct
{
    protected nint ObjectAddress;

    protected unsafe ref TNativeFunctions Functions
    {
        get
        {
            ref var iface = ref Unsafe.AsRef<NativeClass>((void*)ObjectAddress);
            ref var functions = ref Unsafe.AsRef<TNativeFunctions>((void*)iface.VirtualTable);
            return ref functions;
        }
    }

    public override string ToString()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            "Steam Interface<{0}> #{1:X8}",
            typeof(TNativeFunctions),
            ObjectAddress.ToInt32());
    }

    public void SetupFunctions(nint objectAddress)
    {
        ObjectAddress = objectAddress;
    }
}