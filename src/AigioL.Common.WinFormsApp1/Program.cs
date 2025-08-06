using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace AigioL.Common.WinFormsApp1;

static class Program
{
    static GlobalHotkeyListener? hotkeyListener;
    static readonly TaskCompletionSource<SynchronizationContext> tcsAppRun = new();

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            MainCore();
        }
        finally
        {
            hotkeyListener?.Dispose();
        }
    }

    static void MainCore()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        LogInit.InitLog("AigioL.Common", "WinFormsApp1");

        Task.Run(async () =>
        {
            await Task.Delay(1200);

            var ctx = await tcsAppRun.Task;
            var list = new List<HotkeyRegistrationResult>();
            // 在新线程中执行注册热键
            ctx.Send((_) =>
            {
                hotkeyListener = new GlobalHotkeyListener();

                // 订阅事件
                hotkeyListener.HotkeyPressed += (sender, e) =>
                {
                    var hotkey = e.HotkeyInfo;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 热键被按下: {hotkey.Modifiers} + {hotkey.Key}");
                };

                // 注册热键
                var hotkeyId1 = hotkeyListener.RegisterHotkey(
                    ModifierKeys.Control | ModifierKeys.Alt,
                    VirtualKey.F1,
                    () => Console.WriteLine(">>> Ctrl+Alt+F1 功能执行"),
                    "显示帮助"
                );
                list.Add(hotkeyId1);
                Console.WriteLine($"注册热键【显示帮助】: {hotkeyId1}");

                var hotkeyId2 = hotkeyListener.RegisterHotkey(
                    ModifierKeys.Control | ModifierKeys.Alt,
                    VirtualKey.F2,
                    () => Console.WriteLine(">>> Ctrl+Alt+F2 功能执行"),
                    "显示状态"
                );
                list.Add(hotkeyId2);
                Console.WriteLine($"注册热键【显示状态】: {hotkeyId2}");

                var hotkeyId3 = hotkeyListener.RegisterHotkey(
                    ModifierKeys.Control | ModifierKeys.Alt,
                    VirtualKey.A,
                    () => Console.WriteLine(">>> Ctrl+Alt+A 功能执行"),
                    "截图"
                );
                list.Add(hotkeyId3);
                Console.WriteLine($"注册热键【截图】: {hotkeyId3}");
            }, null);

            Console.WriteLine($"热键注册完成，总数：{list.Count}");
        });

#if DEBUG
        // 附加调试运行时，如果不开窗体，退出会卡住
        var f = new Form1();
        tcsAppRun.SetResult(SynchronizationContext.Current!);
        Application.Run(f);
#else
        Console.WriteLine("按下 CTRL+C 退出");
        ApplicationContext ctx = new();
        tcsAppRun.SetResult(SynchronizationContext.Current!);
        Console.CancelKeyPress += (_, e) =>
        {
            if (!e.Cancel)
            {
                ctx.Dispose();
            }
        };

        Application.Run(ctx);
#endif
    }
}