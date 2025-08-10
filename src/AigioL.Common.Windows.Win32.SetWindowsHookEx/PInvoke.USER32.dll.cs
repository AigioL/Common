#if WINDOWS
using global::System.Diagnostics.CodeAnalysis;
using global::System.Runtime.CompilerServices;
using global::System.Runtime.InteropServices;
using global::System.Runtime.Versioning;
#pragma warning disable CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。
using winmdroot = global::Windows.Win32;
#pragma warning restore CS8981 // 该类型名称仅包含小写 ascii 字符。此类名称可能会成为该语言的保留值。

namespace Windows.Win32;

unsafe partial class PInvoke
{
    /// <summary>Installs an application-defined hook procedure into a hook chain. (Unicode)</summary>
    /// <param name="idHook">Type: <b>int</b></param>
    /// <param name="lpfn">
    /// <para>Type: <b>HOOKPROC</b> A pointer to the hook procedure. If the <i>dwThreadId</i> parameter is zero or specifies the identifier of a thread created by a different process, the <i>lpfn</i> parameter must point to a hook procedure in a DLL. Otherwise, <i>lpfn</i> can point to a hook procedure in the code associated with the current process.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="hmod">
    /// <para>Type: <b>HINSTANCE</b> A handle to the DLL containing the hook procedure pointed to by the <i>lpfn</i> parameter. The <i>hMod</i> parameter must be set to <b>NULL</b> if the <i>dwThreadId</i> parameter specifies a thread created by the current process and if the hook procedure is within the code associated with the current process.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="dwThreadId">
    /// <para>Type: <b>DWORD</b> The identifier of the thread with which the hook procedure is to be associated. For desktop apps, if this parameter is zero, the hook procedure is associated with all existing threads running in the same desktop as the calling thread. For Windows Store apps, see the Remarks section.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowshookexw#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>HHOOK</b> If the function succeeds, the return value is the handle to the hook procedure. If the function fails, the return value is <b>NULL</b>. To get extended error information, call <a href="https://docs.microsoft.com/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
    /// </returns>
    /// <remarks>
    /// <para><b>SetWindowsHookEx</b> can be used to inject a DLL into another process. A 32-bit DLL cannot be injected into a 64-bit process, and a 64-bit DLL cannot be injected into a 32-bit process. If an application requires the use of hooks in other processes, it is required that a 32-bit application call <b>SetWindowsHookEx</b> to inject a 32-bit DLL into 32-bit processes, and a 64-bit application call <b>SetWindowsHookEx</b> to inject a 64-bit DLL into 64-bit processes. The 32-bit and 64-bit DLLs must have different names.</para>
    /// <para>Because hooks run in the context of an application, they must match the "bitness" of the application. If a 32-bit application installs a global hook on 64-bit Windows, the 32-bit hook is injected into each 32-bit process (the usual security boundaries apply). In a 64-bit process, the threads are still marked as "hooked." However, because a 32-bit application must run the hook code, the system executes the hook in the hooking app's context; specifically, on the thread that called <b>SetWindowsHookEx</b>. This means that the hooking application must continue to pump messages or it might block the normal functioning of the 64-bit processes.</para>
    /// <para>If a 64-bit application installs a global hook on 64-bit Windows, the 64-bit hook is injected into each 64-bit process, while all 32-bit processes use a callback to the hooking application.</para>
    /// <para>To hook all applications on the desktop of a 64-bit Windows installation, install a 32-bit global hook and a 64-bit global hook, each from appropriate processes, and be sure to keep pumping messages in the hooking application to avoid blocking normal functioning. If you already have a 32-bit global hooking application and it doesn't need to run in each application's context, you may not need to create a 64-bit version.</para>
    /// <para>An error may occur if the <i>hMod</i> parameter is <b>NULL</b> and the <i>dwThreadId</i> parameter is zero or specifies the identifier of a thread created by another process. Calling the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-callnexthookex">CallNextHookEx</a> function to chain to the next hook procedure is optional, but it is highly recommended; otherwise, other applications that have installed hooks will not receive hook notifications and may behave incorrectly as a result. You should call <b>CallNextHookEx</b> unless you absolutely need to prevent the notification from being seen by other applications. Before terminating, an application must call the <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-unhookwindowshookex">UnhookWindowsHookEx</a> function to free system resources associated with the hook. The scope of a hook depends on the hook type. Some hooks can be set only with global scope; others can also be set for only a specific thread, as shown in the following table. </para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-setwindowshookexw#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    [SupportedOSPlatform("windows5.0")]
    internal static nint オペレーティングシステムフックを設定する(int idHook, delegate* unmanaged[Stdcall]<int, winmdroot.Foundation.WPARAM, winmdroot.Foundation.LPARAM, winmdroot.Foundation.LRESULT> lpfn, winmdroot.Foundation.HINSTANCE hmod, uint dwThreadId)
    {
        var libPtr = NativeLibraryE.Load(
           // USER32.dll
           "4837D96C6128D45B617711BC2605D43D"u8
        );
        var methodPtr = NativeLibraryE.GetExport(libPtr,
            // SetWindowsHookExW
            "E6EBFA47091479965EB5F5BDE2E520587733F37DF5123FCB34E6AF3E058C7B28"u8
        );

        var func = (delegate* unmanaged[Stdcall]<int, nint, void*, uint, nint>)methodPtr;
        int __lastError;
        nint __retVal;
        {
            global::System.Runtime.InteropServices.Marshal.SetLastSystemError(0);
            __retVal = func(idHook, (nint)lpfn, hmod.Value, dwThreadId);
            __lastError = global::System.Runtime.InteropServices.Marshal.GetLastSystemError();
        }
        global::System.Runtime.InteropServices.Marshal.SetLastPInvokeError(__lastError);
        return __retVal;
    }

    /// <summary>Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call this function either before or after processing the hook information.</summary>
    /// <param name="hhk">
    /// <para>Type: <b>HHOOK</b> This parameter is ignored.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="nCode">
    /// <para>Type: <b>int</b> The hook code passed to the current hook procedure. The next hook procedure uses this code to determine how to process the hook information.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="wParam">
    /// <para>Type: <b>WPARAM</b> The <i>wParam</i> value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <param name="lParam">
    /// <para>Type: <b>LPARAM</b> The <i>lParam</i> value passed to the current hook procedure. The meaning of this parameter depends on the type of hook associated with the current hook chain.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-callnexthookex#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>LRESULT</b> This value is returned by the next hook procedure in the chain. The current hook procedure must also return this value. The meaning of the return value depends on the hook type. For more information, see the descriptions of the individual hook procedures.</para>
    /// </returns>
    /// <remarks>
    /// <para>Hook procedures are installed in chains for particular hook types. <b>CallNextHookEx</b> calls the next hook in the chain. Calling <b>CallNextHookEx</b> is optional, but it is highly recommended; otherwise, other applications that have installed hooks will not receive hook notifications and may behave incorrectly as a result. You should call <b>CallNextHookEx</b> unless you absolutely need to prevent the notification from being seen by other applications.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-callnexthookex#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    [SupportedOSPlatform("windows5.0")]
    internal static winmdroot.Foundation.LRESULT 次のフックを呼び出す(nint hhk, int nCode, winmdroot.Foundation.WPARAM wParam, winmdroot.Foundation.LPARAM lParam)
    {
        var libPtr = NativeLibraryE.Load(
           // USER32.dll
           "4837D96C6128D45B617711BC2605D43D"u8
        );
        var methodPtr = NativeLibraryE.GetExport(libPtr,
            // CallNextHookEx
            "8212F92DD4102CBB7D70FB2992111FAB"u8
        );

        var func = (delegate* unmanaged[Stdcall]<nint, int, nuint, nint, nint>)methodPtr;
        nint __retVal;
        {
            __retVal = func(hhk, nCode, wParam.Value, lParam.Value);
        }
        return new(__retVal);
    }

    /// <summary>Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function.</summary>
    /// <param name="hhk">
    /// <para>Type: <b>HHOOK</b> A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to <a href="https://docs.microsoft.com/windows/desktop/api/winuser/nf-winuser-setwindowshookexa">SetWindowsHookEx</a>.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-unhookwindowshookex#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>BOOL</b> If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call <a href="https://docs.microsoft.com/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
    /// </returns>
    /// <remarks>The hook procedure can be in the state of being called by another thread even after <b>UnhookWindowsHookEx</b> returns. If the hook procedure is not being called concurrently, the hook procedure is removed immediately before <b>UnhookWindowsHookEx</b> returns.</remarks>
    [SupportedOSPlatform("windows5.0")]
    internal static winmdroot.Foundation.BOOL オペレーティングシステムのフックを解除する(nint hhk)
    {
        var libPtr = NativeLibraryE.Load(
           // USER32.dll
           "4837D96C6128D45B617711BC2605D43D"u8
        );
        var methodPtr = NativeLibraryE.GetExport(libPtr,
            // UnhookWindowsHookEx
            "6AB988EF96DCC0CBAFFA1B2160DB883652B59A0D6B701E3778F9135072543386"u8
        );

        var func = (delegate* unmanaged[Stdcall]<nint, winmdroot.Foundation.BOOL>)methodPtr;
        int __lastError;
        winmdroot.Foundation.BOOL __retVal;
        {
            global::System.Runtime.InteropServices.Marshal.SetLastSystemError(0);
            __retVal = func(hhk);
            __lastError = global::System.Runtime.InteropServices.Marshal.GetLastSystemError();
        }
        global::System.Runtime.InteropServices.Marshal.SetLastPInvokeError(__lastError);
        return __retVal;
    }

    /// <summary>Determines whether a key is up or down at the time the function is called, and whether the key was pressed after a previous call to GetAsyncKeyState.</summary>
    /// <param name="vKey">
    /// <para>Type: <b>int</b> The virtual-key code. For more information, see <a href="https://docs.microsoft.com/windows/desktop/inputdev/virtual-key-codes">Virtual Key Codes</a>. You can use left- and right-distinguishing constants to specify certain keys. See the Remarks section for further information.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getasynckeystate#parameters">Read more on docs.microsoft.com</see>.</para>
    /// </param>
    /// <returns>
    /// <para>Type: <b>SHORT</b> If the function succeeds, the return value specifies whether the key was pressed since the last call to <b>GetAsyncKeyState</b>, and whether the key is currently up or down. If the most significant bit is set, the key is down, and if the least significant bit is set, the key was pressed after the previous call to <b>GetAsyncKeyState</b>. However, you should not rely on this last behavior; for more information, see the Remarks. The return value is zero for the following cases: </para>
    /// <para>This doc was truncated.</para>
    /// </returns>
    /// <remarks>
    /// <para>The <b>GetAsyncKeyState</b> function works with mouse buttons. However, it checks on the state of the physical mouse buttons, not on the logical mouse buttons that the physical buttons are mapped to. For example, the call <b>GetAsyncKeyState</b>(VK_LBUTTON) always returns the state of the left physical mouse button, regardless of whether it is mapped to the left or right logical mouse button. You can determine the system's current mapping of physical mouse buttons to logical mouse buttons by calling <c>GetSystemMetrics(SM_SWAPBUTTON)</c>. which returns TRUE if the mouse buttons have been swapped. Although the least significant bit of the return value indicates whether the key has been pressed since the last query, due to the preemptive multitasking nature of Windows, another application can call <b>GetAsyncKeyState</b> and receive the "recently pressed" bit instead of your application. The behavior of the least significant bit of the return value is retained strictly for compatibility with 16-bit Windows applications (which are non-preemptive) and should not be relied upon. You can use the virtual-key code constants <b>VK_SHIFT</b>, <b>VK_CONTROL</b>, and <b>VK_MENU</b> as values for the <i>vKey</i> parameter. This gives the state of the SHIFT, CTRL, or ALT keys without distinguishing between left and right. You can use the following virtual-key code constants as values for <i>vKey</i> to distinguish between the left and right instances of those keys. </para>
    /// <para>This doc was truncated.</para>
    /// <para><see href="https://learn.microsoft.com/windows/win32/api/winuser/nf-winuser-getasynckeystate#">Read more on docs.microsoft.com</see>.</para>
    /// </remarks>
    [SupportedOSPlatform("windows5.0")]
    internal static short 非同期キー状態を取得する(int vKey)
    {
        var libPtr = NativeLibraryE.Load(
           // USER32.dll
           "4837D96C6128D45B617711BC2605D43D"u8
        );
        var methodPtr = NativeLibraryE.GetExport(libPtr,
            // GetAsyncKeyState
            "763F4FCB87699EE6C54F684A9BA56F2139AE1B07A4F523452DDADCC1316013A1"u8
        );

        var func = (delegate* unmanaged[Stdcall]<int, short>)methodPtr;
        short __retVal;
        {
            __retVal = func(vKey);
        }
        return __retVal;
    }
}
#endif