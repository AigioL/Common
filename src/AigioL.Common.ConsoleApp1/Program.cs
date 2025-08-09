#if WINDOWS
using System.Management.NetworkInformation;
using Windows.Win32.System.Threading;
#endif

namespace AigioL.Common.ConsoleApp1;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        LogInit.InitLog("AigioL.Common.ConsoleApp1");

#if WINDOWS
        Console.WriteLine("\n=== Win32 API 方式 ===");
        TestNetworkAdapterUsage();
        Console.Write("MacAddressHash: ");
        Console.WriteLine(NetAdapterHelper.GetMacAddressHash());

        Console.WriteLine("按下 CTRL+C 退出");
        LightMessageLoop messageLoop = new();
        // 订阅事件
        messageLoop.Idle += (sender, e) =>
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 消息循环空闲");
        };
        messageLoop.UnhandledException += (sender, e) =>
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 未处理异常: {(e.ExceptionObject is Exception ex ? ex.Message : null)}");
        };
        //messageLoop.Exited += (sender, e) =>
        //{
        //    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] 消息循环已退出");
        //};
        Console.CancelKeyPress += (_, e) =>
        {
            if (!e.Cancel)
            {
                messageLoop.Dispose();
                //Environment.Exit(0);
            }
        };
        messageLoop.Run();

#if DEBUG
        Environment.Exit(0);
#endif
#else
        Console.ReadLine();
#endif
    }

#if WINDOWS
    /// <summary>
    /// 测试如何使用 NetworkAdapterHelper（WMI 方式）
    /// </summary>
    static void TestNetworkAdapterUsage()
    {
        Console.WriteLine("=== 网络适配器信息 (WMI) ===");

        // 获取所有网络适配器
        var adapters = NetAdapterHelper.GetAll() ?? [];

        Console.WriteLine($"找到 {adapters.Count} 个网络适配器：\n");

        foreach (var adapter in adapters)
        {
            Console.WriteLine($"名称: {adapter.Name}");
            Console.WriteLine($"描述: {adapter.InterfaceDescription}");
            Console.WriteLine($"索引: {adapter.InterfaceIndex}");
            Console.WriteLine($"状态: {adapter.AdminStatusText} / {adapter.OperationalStatusText}");
            Console.WriteLine($"速度: {adapter.FormattedLinkSpeed}");
            Console.WriteLine($"MAC: {adapter.MacAddress}");
            Console.WriteLine($"虚拟: {(adapter.Virtual ? "是" : "否")}");
            Console.WriteLine($"驱动: {adapter.DriverName} ({adapter.DriverVersion})");
            Console.WriteLine(new string('-', 50));
        }

        // 查找特定适配器
        var ethernetAdapter = adapters.FirstOrDefault(a => a.Name != null && (
            a.Name.Contains("以太网") || a.Name.Contains("Ethernet")));

        if (ethernetAdapter != null)
        {
            Console.WriteLine($"\n找到以太网适配器: {ethernetAdapter.Name}");
        }

        // 按特定条件筛选
        var physicalAdapters = adapters.Where(a => !a.Virtual).ToList();
        Console.WriteLine($"\n物理适配器数量: {physicalAdapters.Count}");

        var vAdapters = adapters.Where(a => a.Virtual).ToList();
        Console.WriteLine($"虚拟适配器数量: {vAdapters.Count}");

        var connectedAdapters = adapters.Where(a => a.OperationalStatus == 1).ToList();
        Console.WriteLine($"已连接适配器数量: {connectedAdapters.Count}");
    }
#endif
}
