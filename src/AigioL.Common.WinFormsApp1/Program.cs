using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace AigioL.Common.WinFormsApp1;

static class Program
{
    static KeyboardHook? hotkeyListener;
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

        LogInit.InitLog("AigioL.Common.WinFormsApp1");

        Task.Run(async () =>
        {
            await Task.Delay(1200);

            var ctx = await tcsAppRun.Task;
            IReadOnlyList<HotkeyRegistrationResult> list = null!;
            // 在新线程中执行注册热键
            ctx.Send((_) =>
            {
                hotkeyListener = KeyboardHook.Instance;

                // 订阅事件
                hotkeyListener.HotkeyPressed += (sender, e) =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 热键被按下: {e.ComboString}");
                };

                ValueTuple<uint, uint, uint>[] hotkeys =
                {
                    ((uint)VirtualKey.Control, (uint)VirtualKey.F1, 0),
                    ((uint)VirtualKey.LControl, (uint)VirtualKey.Alt, (uint)VirtualKey.F1),
                    ((uint)VirtualKey.Control, (uint)VirtualKey.Alt, (uint)VirtualKey.F2),
                    ((uint)VirtualKey.Control, (uint)VirtualKey.Alt, (uint)VirtualKey.A),
                    ((uint)VirtualKey.NumPad0, 0, 0),
                    ((uint)VirtualKey.NumPad1, 0, 0),
                    ((uint)VirtualKey.NumPad2, 0, 0),
                    ((uint)VirtualKey.NumPad3, 0, 0),
                    (0, (uint)VirtualKey.NumPad3, 0),
                    (0, 0,  (uint)VirtualKey.NumPad4),
                    (0, 0,  (uint)VirtualKey.NumPad5),
                    ((uint)VirtualKey.NumPad6, 0, 0),
                    (0, 0,  (uint)VirtualKey.NumPad7),
                    ((uint)VirtualKey.NumPad8, 0, 0),
                    ((uint)VirtualKey.NumPad9, 0, 0),
                    ((uint)VirtualKey.NumPad0, 0, 0),
                };

                // 注册热键
                list = hotkeyListener.RegisterHotkeys(hotkeys);
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