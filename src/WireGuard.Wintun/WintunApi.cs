#pragma warning disable CS0419 // cref 特性中有不明确的引用
using System.Runtime.InteropServices;

namespace WireGuard.Wintun;

/// <summary>
/// Wintun 本机 API 函数
/// <para>Wintun 是一个非常简单、最小的 Windows 内核 TUN 驱动程序，它为用户空间程序提供了一个简单的网络适配器，用于读取和写入数据包</para>
/// <para>它类似于 Linux 和 BSD 的 Wintun 最初设计用于 WireGuard，通常用于各种第 3 层网络协议和实验</para>
/// <para>该驱动程序是开源的，因此任何人都可以检查和构建它</para>
/// <para>由于 Microsoft 的驱动程序签名要求，我们提供预编译和签名版本，这些版本可能会与您的软件一起分发</para>
/// <para>项目的目标是尽可能简单，选择以 NDIS 提供的最纯粹、最直接的方式做事</para>
/// <para>https://www.wintun.net</para>
/// </summary>
public static unsafe partial class WintunApi
{
    /// <summary>
    /// 最小环形缓冲区容量 128KiB
    /// </summary>
    public const uint MinRingCapacity = 0x20000;

    /// <summary>
    /// 最大环形缓冲区容量 64MiB
    /// </summary>
    public const uint MaxRingCapacity = 0x4000000;

    /// <summary>
    /// 最大 IP 数据包大小
    /// </summary>
    public const uint MaxIpPacketSize = 0xFFFF;

    /// <summary>
    /// 适配器名称最大长度
    /// </summary>
    public const int MaxAdapterName = 256;

    /// <summary>
    /// 创建新的 Wintun 适配器
    /// </summary>
    /// <param name="name">请求的适配器名称，以零结尾，最多 <see cref="MaxAdapterName"/>-1 个字符</param>
    /// <param name="tunnelType">适配器隧道类型名称，以零结尾，最多 <see cref="MaxAdapterName"/>-1 个字符</param>
    /// <param name="requestedGuid">要创建的网络适配器 <see cref="Guid"/>，用于确定性影响 NLA 生成</param>
    /// <returns>成功时返回适配器句柄，需使用 <see cref="SafeHandle.Dispose()"/> 释放；失败时返回空句柄，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息</returns>
    [LibraryImport("wintun", EntryPoint = "WintunCreateAdapter", SetLastError = true,
       StringMarshalling = StringMarshalling.Utf16)]
    public static partial WintunAdapterSafeHandle CreateAdapter(string name, string tunnelType, in Guid requestedGuid);

    /// <summary>
    /// 创建新的 Wintun 适配器
    /// </summary>
    /// <param name="name">请求的适配器名称，以零结尾，最多 <see cref="MaxAdapterName"/>-1 个字符</param>
    /// <param name="tunnelType">适配器隧道类型名称，以零结尾，最多 <see cref="MaxAdapterName"/>-1 个字符</param>
    /// <param name="requestedGuid">传入空指针时由系统随机选择 <see cref="Guid"/>，因此每次新建适配器都会创建新的 NLA 条目</param>
    /// <returns>成功时返回适配器句柄，需使用 <see cref="SafeHandle.Dispose()"/> 释放；失败时返回空句柄，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息</returns>
    [LibraryImport("wintun", EntryPoint = "WintunCreateAdapter", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    public static partial WintunAdapterSafeHandle CreateAdapter(string name, string tunnelType, nint requestedGuid);

    /// <summary>
    /// 打开现有的 Wintun 适配器
    /// </summary>
    /// <param name="name">请求的适配器名称，以零结尾，最多 <see cref="MaxAdapterName"/>-1 个字符</param>
    /// <returns>成功时返回适配器句柄，需使用 <see cref="SafeHandle.Dispose()"/> 释放；失败时返回空句柄，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息</returns>
    [LibraryImport("wintun", EntryPoint = "WintunOpenAdapter", SetLastError = true,
        StringMarshalling = StringMarshalling.Utf16)]
    public static partial WintunAdapterSafeHandle OpenAdapter(string name);

    /// <summary>
    /// 释放 Wintun 适配器资源；若适配器由 <see cref="CreateAdapter"/> 创建则同时移除该适配器
    /// </summary>
    /// <param name="adapter">由 <see cref="CreateAdapter"/> 或 <see cref="OpenAdapter"/> 获取的适配器句柄</param>
    [LibraryImport("wintun", EntryPoint = "WintunCloseAdapter")]
    internal static partial void CloseAdapter(nint adapter);

    /// <summary>
    /// 当没有适配器在使用时删除 Wintun 驱动
    /// </summary>
    /// <returns>成功返回 <see langword="true"/>，失败返回 <see langword="false"/>，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息</returns>
    [LibraryImport("wintun", EntryPoint = "WintunDeleteDriver", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteDriver();

    /// <summary>
    /// 获取适配器的 LUID
    /// </summary>
    /// <param name="adapter">由 <see cref="CreateAdapter"/> 或 <see cref="OpenAdapter"/> 获取的适配器句柄</param>
    /// <param name="luId">接收适配器 LUID 的输出参数</param>
    [LibraryImport("wintun", EntryPoint = "WintunGetAdapterLUID")]
    public static partial void GetAdapterLuid(WintunAdapterSafeHandle adapter, out ulong luId);

    /// <summary>
    /// 获取当前已加载 Wintun 驱动的版本号
    /// </summary>
    /// <returns>成功时返回版本号，失败时返回 0，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息，可能为 ERROR_FILE_NOT_FOUND 表示 Wintun 未加载</returns>
    [LibraryImport("wintun", EntryPoint = "WintunGetRunningDriverVersion")]
#pragma warning disable CA1401 // P/Invokes 应该是不可见的
    public static partial uint GetRunningDriverVersion();
#pragma warning restore CA1401 // P/Invokes 应该是不可见的

    /// <summary>
    /// 设置日志回调函数
    /// </summary>
    /// <param name="newLog">新的全局日志回调，可能被多个线程并发调用；若需要串行化需在回调内自行处理；传入 <see langword="null"/> 可禁用日志</param>
    [LibraryImport("wintun", EntryPoint = "WintunSetLogger")]
    public static partial void SetLogger([MarshalAs(UnmanagedType.FunctionPtr)] WintunLogCallback? newLog);

    /// <summary>
    /// 启动 Wintun 会话
    /// </summary>
    /// <param name="adapter">由 <see cref="OpenAdapter"/> 或 <see cref="CreateAdapter"/> 获取的适配器句柄</param>
    /// <param name="capacity">环形缓冲区容量，必须在 <see cref="MinRingCapacity"/> 到 <see cref="MaxRingCapacity"/> 范围内且为 2 的幂</param>
    /// <returns>成功时返回会话句柄，需使用 <see cref="EndSession"/> 释放；失败时返回空句柄，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息</returns>
    [LibraryImport("wintun", EntryPoint = "WintunStartSession", SetLastError = true)]
    public static partial WintunSessionSafeHandle StartSession(WintunAdapterSafeHandle adapter, uint capacity);

    /// <summary>
    /// 结束 Wintun 会话
    /// </summary>
    /// <param name="session">由 <see cref="StartSession"/> 获取的会话句柄</param>
    [LibraryImport("wintun", EntryPoint = "WintunEndSession")]
    internal static partial void EndSession(nint session);

    /// <summary>
    /// 获取会话的读等待事件句柄
    /// </summary>
    /// <param name="session">由 <see cref="StartSession"/> 获取的会话句柄</param>
    /// <returns>返回用于等待可读数据的事件句柄；当 <see cref="ReceivePacket"/> 返回 ERROR_NO_MORE_ITEMS 时应等待该事件后重试；不要对该事件调用 CloseHandle</returns>
    [LibraryImport("wintun", EntryPoint = "WintunGetReadWaitEvent")]
    public static partial nint GetReadWaitEvent(WintunSessionSafeHandle session);

    /// <summary>
    /// 接收一个数据包，处理完成后需调用 <see cref="ReleaseReceivePacket"/> 释放内部缓冲区，此函数线程安全
    /// </summary>
    /// <param name="session">由 <see cref="StartSession"/> 获取的会话句柄</param>
    /// <param name="packetSize">接收数据包大小的输出参数</param>
    /// <returns>成功时返回三层 IPv4 或 IPv6 数据包指针，内容可修改；失败时返回空指针，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息，可能包括 ERROR_HANDLE_EOF、ERROR_NO_MORE_ITEMS、ERROR_INVALID_DATA</returns>
    [LibraryImport("wintun", EntryPoint = "WintunReceivePacket", SetLastError = true)]
    public static partial byte* ReceivePacket(WintunSessionSafeHandle session, out uint packetSize);

    /// <summary>
    /// 在客户端处理完接收包后释放内部缓冲区，此函数线程安全
    /// </summary>
    /// <param name="session">由 <see cref="StartSession"/> 获取的会话句柄</param>
    /// <param name="packet">由 <see cref="ReceivePacket"/> 获取的数据包指针</param>
    [LibraryImport("wintun", EntryPoint = "WintunReleaseReceivePacket")]
    public static partial void ReleaseReceivePacket(WintunSessionSafeHandle session, byte* packet);

    /// <summary>
    /// 为发送数据包分配内存，填充完成后调用 <see cref="SendPacket"/> 发送并释放内部缓冲区，此函数线程安全，分配调用顺序决定发送顺序
    /// </summary>
    /// <param name="session">由 <see cref="StartSession"/> 获取的会话句柄</param>
    /// <param name="packetSize">精确的数据包大小，必须小于或等于 <see cref="MaxIpPacketSize"/></param>
    /// <returns>成功时返回用于准备三层 IPv4 或 IPv6 发送包的内存指针；失败时返回空指针，可通过 <see cref="Marshal.GetLastWin32Error"/> 获取扩展错误信息，可能包括 ERROR_HANDLE_EOF、ERROR_BUFFER_OVERFLOW</returns>
    [LibraryImport("wintun", EntryPoint = "WintunAllocateSendPacket", SetLastError = true)]
    public static partial byte* AllocateSendPacket(WintunSessionSafeHandle session, uint packetSize);

    /// <summary>
    /// 发送数据包并释放内部缓冲区，此函数线程安全，但发送顺序由 <see cref="AllocateSendPacket"/> 的调用顺序决定，不保证严格按 <see cref="SendPacket"/> 调用顺序发送
    /// </summary>
    /// <param name="session">由 <see cref="StartSession"/> 获取的会话句柄</param>
    /// <param name="packet">由 <see cref="AllocateSendPacket"/> 获取的数据包指针</param>
    [LibraryImport("wintun", EntryPoint = "WintunSendPacket")]
    public static partial void SendPacket(WintunSessionSafeHandle session, byte* packet);
}
