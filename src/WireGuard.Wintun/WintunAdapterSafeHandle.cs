using System.Runtime.InteropServices;

namespace WireGuard.Wintun;

public sealed partial class WintunAdapterSafeHandle(nint handle) : SafeHandle(handle, true)
{
    public WintunAdapterSafeHandle() : this(default)
    {
    }

    protected sealed override bool ReleaseHandle()
    {
        WintunApi.CloseAdapter(handle);
        return true;
    }

    public sealed override bool IsInvalid => handle == default;
}
