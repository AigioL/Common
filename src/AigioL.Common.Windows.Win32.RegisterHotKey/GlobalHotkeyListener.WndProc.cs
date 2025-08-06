#if WINDOWS
using Windows.Win32.Foundation;

namespace Windows.Win32.UI.Input.KeyboardAndMouse;

partial class GlobalHotkeyListener
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-hotkey
    /// </summary>
    const uint WM_HOTKEY = 0x0312;
}

#if USE_WINDOWSFORMS
partial class GlobalHotkeyListener
{
    WndProcDelegateWindow _hwnd;

    void WndProc(ref global::System.Windows.Forms.Message m)
    {
        var msg = unchecked((uint)m.Msg);
        if (msg == WM_HOTKEY)
        {
            var hotkeyId = m.WParam.ToInt32();
            OnHotkeyPressed(hotkeyId);
            m.Result = 0; // 返回值
        }
    }

    sealed class WndProcDelegateWindow : global::System.Windows.Forms.NativeWindow
    {
        internal delegate void WndProcDelegate(ref global::System.Windows.Forms.Message m);

        readonly WndProcDelegate d;

        internal WndProcDelegateWindow(WndProcDelegate d)
        {
            this.d = d;
        }

        protected unsafe sealed override void WndProc(ref global::System.Windows.Forms.Message m)
        {
            d.Invoke(ref m);
            base.WndProc(ref m);
        }

        public static implicit operator HWND(WndProcDelegateWindow w) => new(w.Handle);

        public static implicit operator nint(WndProcDelegateWindow w) => new(w.Handle);
    }

    void DestroyWindow()
    {
        if (_hwnd != null)
        {
            _hwnd.DestroyHandle();
            _hwnd = null!;
        }
    }
}
#else
partial class GlobalHotkeyListener
{
    HWND _hwnd;

    void CreateMessageWindow()
    {
        throw new NotImplementedException("直接调用 Win32 API 方案未实现");
        //var wndClass = new WNDCLASSEXW
        //{
        //    cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
        //    lpfnWndProc = WindowProc,
        //    hInstance = PInvoke.GetModuleHandle((string?)null),
        //    lpszClassName = "GlobalHotkeyListener_MessageWindow"
        //};

        //var classAtom = PInvoke.RegisterClassEx(wndClass);
        //if (classAtom == 0)
        //{
        //    throw new Win32Exception(Marshal.GetLastWin32Error(), "注册窗口类失败");
        //}

        //_hwnd = PInvoke.CreateWindowEx(
        //    0,
        //    classAtom,
        //    "GlobalHotkeyListener",
        //    0,
        //    0, 0, 0, 0,
        //    HWND.HWND_MESSAGE,
        //    HMENU.Null,
        //    wndClass.hInstance,
        //    null
        //);

        //if (_hwnd.IsNull)
        //{
        //    throw new Win32Exception(Marshal.GetLastWin32Error(), "创建消息窗口失败");
        //}
    }

    LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        throw new NotImplementedException("直接调用 Win32 API 方案未实现");
        //if (msg == WM_HOTKEY)
        //{
        //    var hotkeyId = unchecked((int)wParam.Value.ToUInt32());
        //    OnHotkeyPressed(hotkeyId);
        //    return new LRESULT(0);
        //}

        //return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
    }

    void RunMessageLoop()
    {
        throw new NotImplementedException("直接调用 Win32 API 方案未实现");
        //try
        //{
        //    var cancellationToken = _cancellationTokenSource.Token;

        //    while (!cancellationToken.IsCancellationRequested && !_disposed)
        //    {
        //        var hasMessage = PInvoke.PeekMessage(out var msg, _hwnd, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE);

        //        if (hasMessage)
        //        {
        //            PInvoke.TranslateMessage(msg);
        //            PInvoke.DispatchMessage(msg);
        //        }
        //        else
        //        {
        //            Thread.Sleep(10);
        //        }
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Debug.WriteLine($"消息循环异常: {ex}");
        //}
    }

    void DestroyWindow()
    {
        throw new NotImplementedException("直接调用 Win32 API 方案未实现");
        //if (!_hwnd.IsNull)
        //{
        //    PInvoke.DestroyWindow(_hwnd);
        //}
    }
}
#endif
#endif