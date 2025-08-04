#if WINDOWS
using System.Management.NetworkInformation;

namespace AigioL.Common.UnitTest;

public sealed class NetAdapterUnitTest : BaseUnitTest
{
    [Fact]
    public void GetMacAddressHash()
    {
        var value = NetAdapterHelper.GetMacAddressHash("NetAdapterUnitTest"u8);
        Assert.NotNull(value);
        Console.Write("MacAddressHash: ");
        Console.WriteLine(value);
    }

    /// <summary>
    /// 测试如何使用 NetworkAdapterHelper（WMI 方式）
    /// </summary>
    [Fact]
    public void TestNetworkAdapterUsage()
    {
        Console.WriteLine("=== 网络适配器信息 (WMI) ===");

        // 获取所有网络适配器
        var adapters = NetAdapterHelper.GetAll();
        Assert.True(adapters != null && adapters.Count > 0, "未找到任何网络适配器");

        //TestContext.Current.w
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
}
#endif