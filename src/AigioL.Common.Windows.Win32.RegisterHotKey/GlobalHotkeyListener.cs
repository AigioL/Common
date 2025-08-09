#if WINDOWS
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32.UI.Input.KeyboardAndMouse;

/// <summary>
/// 全局热键监听器
/// </summary>
public sealed partial class GlobalHotkeyListener : IDisposable
{
    readonly Dictionary<int, HotkeyInfo> _registeredHotkeys = new();
    readonly Dictionary<HotkeyCombo, int> _comboToId = new();
    readonly Lock _lockHotkeys = new();
    int _nextHotkeyId = 1;
    bool _disposed;
    readonly CancellationTokenSource _cancellationTokenSource = new();
    readonly ILogger logger;

    public GlobalHotkeyListener()
    {
        logger = Log.CreateLogger<GlobalHotkeyListener>();
#if USE_WINDOWSFORMS
        _hwnd = new(WndProc);
        _hwnd.CreateHandle(new());
#else
        CreateMessageWindow();
        Task.Factory.StartNew(RunMessageLoop, TaskCreationOptions.LongRunning);
#endif
    }

    const int ERROR_HOTKEY_ALREADY_REGISTERED = 0x581;
    const int ERROR_INVALID_WINDOW_HANDLE = 0x578;
    const int ERROR_INVALID_PARAMETER = 0x57;

    /// <summary>
    /// 注册全局热键
    /// </summary>
    /// <param name="modifiers">修饰键</param>
    /// <param name="key">主键</param>
    /// <param name="action">按下热键时执行的动作</param>
    /// <param name="description">热键描述</param>
    /// <returns>注册结果</returns>
    public HotkeyRegistrationResult RegisterHotkey(
        ModifierKeys modifiers,
        VirtualKey key,
        Action? action,
        string? description = null)
    {
        ThrowIfDisposed();

        var combo = new HotkeyCombo(modifiers, key);

        lock (_lockHotkeys)
        {
            // 检查是否已经注册了相同的组合
            if (_comboToId.ContainsKey(combo))
            {
                return new HotkeyRegistrationResult
                {
                    Success = false,
                    NativeErrorCode = ERROR_HOTKEY_ALREADY_REGISTERED,
                    Message = $"热键 {combo} 已在此应用程序中注册",
                    Combo = combo,
                    ErrorCode = HotkeyRegistrationError.AlreadyRegisteredByThisApp,
                };
            }

            var hotkeyId = _nextHotkeyId++;
            var success = PInvoke.RegisterHotKey(_hwnd, hotkeyId, (HOT_KEY_MODIFIERS)combo.Modifiers, (uint)combo.Key);

            if (success)
            {
                var registration = new HotkeyInfo(hotkeyId, combo, action, description);
                _registeredHotkeys[hotkeyId] = registration;
                _comboToId[combo] = hotkeyId;

                return new HotkeyRegistrationResult
                {
                    Success = true,
                    HotkeyId = hotkeyId,
                    Combo = combo,
                    Message = $"成功注册热键 {combo}"
                };
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                var errorCode = error switch
                {
                    ERROR_HOTKEY_ALREADY_REGISTERED => HotkeyRegistrationError.AlreadyRegisteredByOtherApp,
                    ERROR_INVALID_WINDOW_HANDLE => HotkeyRegistrationError.InvalidWindowHandle,
                    ERROR_INVALID_PARAMETER => HotkeyRegistrationError.InvalidParameter,
                    _ => HotkeyRegistrationError.Unknown
                };
                var message = errorCode switch
                {
                    HotkeyRegistrationError.AlreadyRegisteredByOtherApp =>
                        $"热键 {combo} 已被其他应用程序注册",
                    HotkeyRegistrationError.InvalidWindowHandle =>
                        "无效的窗口句柄",
                    HotkeyRegistrationError.InvalidParameter =>
                        $"无效的热键参数: {combo}",
                    _ => $"注册热键失败: Win32 错误码 {error}"
                };

                // 触发注册失败事件
                HotkeyRegistrationFailed?.Invoke(this, new HotkeyRegistrationFailedEventArgs(combo, error, errorCode, message));

                return new HotkeyRegistrationResult
                {
                    Success = false,
                    NativeErrorCode = error,
                    ErrorCode = errorCode,
                    Combo = combo,
                    Message = message,
                };
            }
        }
    }

    /// <summary>
    /// 获取所有已注册的热键信息
    /// </summary>
    public IReadOnlyList<HotkeyInfo> GetRegisteredHotkeys()
    {
        lock (_lockHotkeys)
        {
            return [.. _registeredHotkeys.Values];
        }
    }

    /// <summary>
    /// 检查指定热键组合是否可用
    /// </summary>
    public bool IsHotkeyAvailable(ModifierKeys modifiers, VirtualKey key)
    {
        var testId = 99999; // 使用一个不会冲突的测试ID
        var success = PInvoke.RegisterHotKey(_hwnd, testId, (HOT_KEY_MODIFIERS)modifiers, (uint)key);

        if (success)
        {
            PInvoke.UnregisterHotKey(_hwnd, testId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 注销热键
    /// </summary>
    public bool UnregisterHotkey(int hotkeyId)
    {
        ThrowIfDisposed();

        lock (_lockHotkeys)
        {
            if (_registeredHotkeys.TryGetValue(hotkeyId, out var registration))
            {
                var success = PInvoke.UnregisterHotKey(_hwnd, hotkeyId);
                if (success)
                {
                    _registeredHotkeys.Remove(hotkeyId);
                    _comboToId.Remove(registration.Combo);
                }
                return success;
            }
            return false;
        }
    }

    void OnHotkeyPressed(int hotkeyId)
    {
        HotkeyInfo? hotkeyInfo = null;
        lock (_lockHotkeys)
        {
            _registeredHotkeys.TryGetValue(hotkeyId, out hotkeyInfo);
        }
        if (hotkeyInfo != null)
        {
            try
            {
                HotkeyPressed?.Invoke(this, new HotkeyPressedEventArgs(hotkeyInfo));
                hotkeyInfo.Action?.Invoke();
            }
            catch (Exception ex)
            {
                LoggerMessages.OnHotkeyPressedException(logger, ex,
                    hotkeyInfo.Id, hotkeyInfo.Combo, hotkeyInfo.Description);
            }
        }
    }

    void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(GlobalHotkeyListener));
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _cancellationTokenSource.Cancel();

        lock (_lockHotkeys)
        {
            foreach (var kvp in _registeredHotkeys)
            {
                PInvoke.UnregisterHotKey(_hwnd, kvp.Key);
            }
            _registeredHotkeys.Clear();
            _comboToId.Clear();
        }

        DestroyWindow();

        _cancellationTokenSource.Dispose();
    }
}

static partial class LoggerMessages
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "执行热键动作时发生异常，HotkeyId: {hotKeyId}，Combo：{combo}，Description：{description}")]
    internal static partial void OnHotkeyPressedException(this ILogger logger, Exception exception, int hotKeyId, HotkeyCombo combo, string? description);
}
#endif