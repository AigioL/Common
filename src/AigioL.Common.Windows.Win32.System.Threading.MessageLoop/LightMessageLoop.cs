#if WINDOWS
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32.System.Threading;

/// <summary>
/// 轻量级消息循环上下文，参考 WinForms LightThreadContext 实现
/// </summary>
public sealed class LightMessageLoop : IDisposable
{
    readonly Lock _lockObject = new();
    volatile bool _disposed;
    volatile bool _exitRequested;
    volatile bool _running;
    uint _threadId;
    TaskCompletionSource? _exitCompletionSource;
    readonly Dictionary<HWND, MessageLoopCallback> _windowCallbacks = new();

    /// <summary>
    /// 消息处理回调委托
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="msg">消息，类型为结构 global::Windows.Win32.UI.WindowsAndMessaging.MSG 的指针</param>
    /// <returns>如果消息已处理返回 <see langword="true"/>，否则返回 <see langword="false"/></returns>
    public delegate bool MessageLoopCallback(nint msg);

    /// <summary>
    /// 消息循环退出事件
    /// </summary>
    public event EventHandler? Exited;

    /// <summary>
    /// 消息循环空闲事件
    /// </summary>
    public event EventHandler? Idle;

    /// <summary>
    /// 未处理异常事件
    /// </summary>
    public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

    /// <summary>
    /// 当前消息循环是否正在运行
    /// </summary>
    public bool IsRunning => _running;

    /// <summary>
    /// 当前消息循环的线程 Id
    /// </summary>
    public uint ThreadId => _threadId;

    /// <summary>
    /// 注册窗口消息处理回调
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="callback">消息处理回调</param>
    public void RegisterWindow(nint hwnd, MessageLoopCallback callback)
    {
        ThrowIfDisposed();

        lock (_lockObject)
        {
            _windowCallbacks[new HWND(hwnd)] = callback;
        }
    }

    /// <summary>
    /// 注销窗口消息处理回调
    /// </summary>
    /// <param name="hwnd">窗口句柄</param>
    public void UnregisterWindow(nint hwnd)
    {
        ThrowIfDisposed();

        lock (_lockObject)
        {
            _windowCallbacks.Remove(new HWND(hwnd));
        }
    }

    /// <summary>
    /// 运行消息循环（阻塞调用）
    /// </summary>
    public void Run()
    {
        ThrowIfDisposed();

        if (_running)
        {
            throw new InvalidOperationException("消息循环已在运行中");
        }

        _threadId = PInvoke.GetCurrentThreadId();
        _running = true;
        _exitRequested = false;
        _exitCompletionSource = new TaskCompletionSource();

        try
        {
            RunCore();
        }
        finally
        {
            _running = false;
            _exitCompletionSource?.TrySetResult();
            // TODO: 👇，这里注册事件在 Debug 调试运行时卡住死锁？
            Exited?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 异步运行消息循环
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    public Task RunAsync(CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(
            () => RunWithCancellation(cancellationToken),
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    /// <summary>
    /// 退出消息循环
    /// </summary>
    public void Exit()
    {
        if (!_running)
        {
            return;
        }

        _exitRequested = true;

        // 发送 WM_QUIT 消息来退出消息循环
        PInvoke.PostThreadMessage(_threadId, WM_QUIT, default, default);
    }

    /// <summary>
    /// 异步等待消息循环退出
    /// </summary>
    /// <returns></returns>
    public Task WaitForExitAsync()
    {
        ThrowIfDisposed();

        if (!_running)
        {
            return Task.CompletedTask;
        }

        return _exitCompletionSource?.Task ?? Task.CompletedTask;
    }

    /// <summary>
    /// 处理单个消息（非阻塞）
    /// </summary>
    /// <returns>如果处理了消息返回 true，否则返回 false</returns>
    public unsafe bool ProcessSingleMessage()
    {
        ThrowIfDisposed();

        if (PInvoke.PeekMessage(out var msg, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
        {
            var lpMsgLocal = &msg;
            ProcessMessage(lpMsgLocal);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 处理所有待处理的消息（非阻塞）
    /// </summary>
    /// <returns>处理的消息数量</returns>
    public unsafe int ProcessAllMessages()
    {
        ThrowIfDisposed();

        int processedCount = 0;
        while (PInvoke.PeekMessage(out var msg, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
        {
            var lpMsgLocal = &msg;
            ProcessMessage(lpMsgLocal);
            processedCount++;

            // 防止无限循环
            if (processedCount > 1000)
            {
                break;
            }
        }

        return processedCount;
    }

    void RunWithCancellation(CancellationToken cancellationToken)
    {
        using var registration = cancellationToken.Register(Exit);
        Run();
    }

    unsafe void RunCore()
    {
        bool hasIdleBeenRaised = false;

        while (!_exitRequested && !_disposed)
        {
            try
            {
                // 使用 GetMessage 进行阻塞等待
                var result = PInvoke.GetMessage(out var msg, HWND.Null, 0, 0);

                if (result == 0) // WM_QUIT
                {
                    break;
                }
                else if (result == -1) // 错误
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error, "GetMessage 失败");
                }
                else
                {
                    var lpMsgLocal = &msg;
                    ProcessMessage(lpMsgLocal);
                    hasIdleBeenRaised = false;
                }
            }
            catch (Exception ex) when (!IsUnrecoverableException(ex))
            {
                HandleException(ex);
            }

            // 检查是否需要触发空闲事件
            if (!hasIdleBeenRaised && !PInvoke.PeekMessage(out _, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_NOREMOVE))
            {
                OnIdle();
                hasIdleBeenRaised = true;
            }
        }
    }

    /// <summary>
    /// https://learn.microsoft.com/zh-cn/windows/win32/winmsg/wm-quit
    /// </summary>
    const uint WM_QUIT = 0x0012;

    unsafe void ProcessMessage(MSG* msg)
    {
        bool handled = false;
        var hwnd = msg->hwnd;

        // 首先尝试窗口特定的回调
        if (!hwnd.IsNull)
        {
            lock (_lockObject)
            {
                if (_windowCallbacks.TryGetValue(hwnd, out var callback))
                {
                    try
                    {
                        handled = callback((nint)msg);
                    }
                    catch (Exception ex) when (!IsUnrecoverableException(ex))
                    {
                        HandleException(ex);
                    }
                }
            }
        }

        // 如果没有被处理，使用默认的消息处理
        if (!handled)
        {
            // 检查是否是退出消息
            if (msg->message == WM_QUIT)
            {
                _exitRequested = true;
                return;
            }

            // 标准消息处理
            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }
    }

    void OnIdle()
    {
        try
        {
            Idle?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) when (!IsUnrecoverableException(ex))
        {
            HandleException(ex);
        }
    }

    void HandleException(Exception ex)
    {
        try
        {
            var args = new UnhandledExceptionEventArgs(ex, false);
            UnhandledException?.Invoke(this, args);
        }
        catch
        {
            // 防止异常处理本身引发异常
        }

#if DEBUG
        // 在调试模式下输出异常信息
        Debug.WriteLine($"LightMessageLoop 未处理异常: {ex}");
#endif
    }

    static bool IsUnrecoverableException(Exception ex)
    {
        return ex is OutOfMemoryException ||
               ex is StackOverflowException ||
               ex is AccessViolationException ||
               ex is SEHException;
    }

    void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(LightMessageLoop));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_running)
        {
            Exit();
        }

        lock (_lockObject)
        {
            _windowCallbacks.Clear();
        }

        _exitCompletionSource?.TrySetCanceled();
    }
}
#endif