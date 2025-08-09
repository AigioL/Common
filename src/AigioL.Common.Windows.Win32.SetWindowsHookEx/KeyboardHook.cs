#if WINDOWS
using ABI.System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using HotKeyInfo = global::System.Collections.Generic.KeyValuePair<global::Windows.Win32.UI.Input.KeyboardAndMouse.HotkeyCombo, (uint, uint, uint)>;

namespace Windows.Win32.UI.Input.KeyboardAndMouse;

public sealed partial class KeyboardHook
{
    readonly HHOOK hhk;
    const nuint WM_KEYDOWN = 0x0100;
    readonly int nativeErrorCode;
    readonly Lock _lockHotkeys = new();
    ImmutableArray<HotKeyInfo> _registeredHotkeys;

#pragma warning disable IDE1006 // 命名样式
    ILogger logger => field ??= Log.CreateLogger<KeyboardHook>();
#pragma warning restore IDE1006 // 命名样式

    /// <summary>
    /// 热键按下事件
    /// </summary>
    public event EventHandler<HotkeyPressedEventArgs>? HotkeyPressed;

    public KeyboardHook()
    {
        PCWSTR lpModuleName = default;
        var hmod = PInvoke.GetModuleHandle(lpModuleName);
        hhk = PInvoke.SetWindowsHookEx(WINDOWS_HOOK_ID.WH_KEYBOARD_LL, HookCallback, hmod, 0);
        if (hhk == default)
        {
            nativeErrorCode = Marshal.GetLastWin32Error();
        }
    }

    public HotkeyRegistrationResult[] RegisterHotkeys<T>(params IReadOnlyList<T> hotkeys) where T : struct
    {
        var registeredHotkeys = new HotKeyInfo[hotkeys.Count];
        var result = new HotkeyRegistrationResult[hotkeys.Count];
        for (int i = 0; i < hotkeys.Count; i++)
        {
            var it = hotkeys[i];
            ref var refIt = ref it;
            ref ValueTuple<uint, uint, uint> refTuple = ref Unsafe.As<T, ValueTuple<uint, uint, uint>>(ref refIt);
            ReadOnlySpan<ValueTuple<uint, uint, uint>> span = new(ref refTuple);
            var combo = HotkeyCombo.Create(span[0].Item1, span[0].Item2, span[0].Item3);
            registeredHotkeys[i] = new(combo, span[0]);
            result[i] = new()
            {
                Success = nativeErrorCode == 0,
                Combo = combo,
                NativeErrorCode = nativeErrorCode,
                P0 = span[0].Item1,
                P1 = span[0].Item2,
                P2 = span[0].Item3,
            };
        }
        _registeredHotkeys = [.. registeredHotkeys];
        return result;
    }

    public HotkeyRegistrationResult[] RegisterHotkeys(params IReadOnlyList<ValueTuple<uint, uint, uint>> hotkeys)
    {
        var registeredHotkeys = new HotKeyInfo[hotkeys.Count];
        var result = new HotkeyRegistrationResult[hotkeys.Count];
        for (int i = 0; i < hotkeys.Count; i++)
        {
            var it = hotkeys[i];
            var combo = HotkeyCombo.Create(it.Item1, it.Item2, it.Item3);
            registeredHotkeys[i] = new(combo, it);
            result[i] = new()
            {
                Success = nativeErrorCode == 0,
                Combo = combo,
                NativeErrorCode = nativeErrorCode,
                P0 = it.Item1,
                P1 = it.Item2,
                P2 = it.Item3,
            };
        }
        _registeredHotkeys = [.. registeredHotkeys];
        return result;
    }

    LRESULT HookCallback(int code, WPARAM wParam, LPARAM lParam)
    {
        if (code >= 0 && wParam.Value == WM_KEYDOWN)
        {
            VirtualKey key;
            unsafe
            {
                var kbd = (KBDLLHOOKSTRUCT*)lParam.Value;
                key = (VirtualKey)kbd->vkCode;
            }
            var modifiers = GetCurrentModifiers();

            var registeredHotkeys = _registeredHotkeys;
            if (!registeredHotkeys.IsDefaultOrEmpty)
            {
                foreach (var hotkey in registeredHotkeys)
                {
                    if (hotkey.Key.Key == key && hotkey.Key.Modifiers == modifiers)
                    {
                        try
                        {
                            HotkeyPressed?.Invoke(this, new()
                            {
                                Combo = hotkey.Key,
                                P0 = hotkey.Value.Item1,
                                P1 = hotkey.Value.Item2,
                                P2 = hotkey.Value.Item3,
                            });
                        }
                        catch (Exception ex)
                        {
                            LoggerMessages.OnHotkeyPressedException(logger, ex, hotkey.Key);
                        }
                    }
                }
            }
        }
        // 返回 CallNextHookEx，让系统继续处理
        return PInvoke.CallNextHookEx(hhk, code, wParam, lParam);
    }

    static ModifierKeys GetCurrentModifiers()
    {
        ModifierKeys mods = ModifierKeys.None;
        if ((PInvoke.GetAsyncKeyState(unchecked((int)VirtualKey.Control)) & 0x8000) != 0)
            mods |= ModifierKeys.Control;
        if ((PInvoke.GetAsyncKeyState(unchecked((int)VirtualKey.Shift)) & 0x8000) != 0)
            mods |= ModifierKeys.Shift;
        if ((PInvoke.GetAsyncKeyState(unchecked((int)VirtualKey.Alt)) & 0x8000) != 0)
            mods |= ModifierKeys.Alt;
        if ((PInvoke.GetAsyncKeyState(0x5B) & 0x8000) != 0 || (PInvoke.GetAsyncKeyState(0x5C) & 0x8000) != 0)
            mods |= ModifierKeys.Win;
        return mods;
    }

}

partial class KeyboardHook : IDisposable
{
    bool disposedValue;

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                if (hhk != default)
                {
                    PInvoke.UnhookWindowsHookEx(hhk);
                }
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            _registeredHotkeys = default;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "执行热键动作时发生异常，Combo：{combo}")]
    internal static partial void OnHotkeyPressedException(this ILogger logger, Exception exception, HotkeyCombo combo);
}
#endif