using System.Runtime.InteropServices;

namespace WireGuard.Wintun;

public sealed partial class WintunSessionSafeHandle(nint handle) : SafeHandle(handle, true)
{
    public WintunSessionSafeHandle() : this(default)
    {
    }

    protected sealed override bool ReleaseHandle()
    {
        WintunApi.EndSession(handle);
        return true;
    }

    public sealed override bool IsInvalid => handle == default;
}
