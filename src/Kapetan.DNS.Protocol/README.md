# DNS

A DNS library written in C# targeting .NET Standard 2.0. Versions prior to version two (2.0.0) were written for .NET 4 using blocking network operations. Version two and above use asynchronous operations.

Available through NuGet.

	Install-Package DNS

[![Test](https://github.com/kapetan/dns/actions/workflows/test.yml/badge.svg)](https://github.com/kapetan/dns/actions/workflows/test.yml)

# Usage

The library exposes a `Request` and `Response` classes for parsing and creating DNS messages. These can be serialized to byte arrays.

```C#
Request request = new Request();

request.RecursionDesired = true;
request.Id = 123;

UdpClient udp = new UdpClient();
IPEndPoint google = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);

// Send to google's DNS server
await udp.SendAsync(request.ToArray(), request.Size, google);

UdpReceiveResult result = await udp.ReceiveAsync();
byte[] buffer = result.Buffer;
Response response = Response.FromArray(buffer);

// Outputs a human readable representation
Console.WriteLine(response);
```

### Client

The libray also includes a small client and a proxy server. Using the `ClientRequest` or the `DnsClient` class it is possible to send a request to a Domain Name Server. The request is first sent using UDP, if that fails (response is truncated), the request is sent again using TCP. This behaviour can be changed by supplying an `IRequestResolver` to the client constructor.

```C#
ClientRequest request = new ClientRequest("8.8.8.8");

// Request an IPv6 record for the foo.com domain
request.Questions.Add(new Question(Domain.FromString("foo.com"), RecordType.AAAA));
request.RecursionDesired = true;

IResponse response = await request.Resolve();

// Get all the IPs for the foo.com domain
IList<IPAddress> ips = response.AnswerRecords
	.Where(r => r.Type == RecordType.AAAA)
	.Cast<IPAddressResourceRecord>()
	.Select(r => r.IPAddress)
	.ToList();
```

The `DnsClient` class contains some conveniance methods for creating instances of `ClientRequest` and resolving domains.

```C#
// Bind to a Domain Name Server
DnsClient client = new DnsClient("8.8.8.8");

// Create request bound to 8.8.8.8
ClientRequest request = client.Create();

// Returns a list of IPs
IList<IPAddress> ips = await client.Lookup("foo.com");

// Get the domain name belonging to the IP (google.com)
string domain = await client.Reverse("173.194.69.100");
```

### Server

The `DnsServer` class exposes a proxy Domain Name Server (UDP only). You can intercept domain name resolution requests and route them to specified IPs. The server is asynchronous. It also emits an event on every request and every successful resolution.

```C#
// Proxy to google's DNS
MasterFile masterFile = new MasterFile();
DnsServer server = new DnsServer(masterFile, "8.8.8.8");

// Resolve these domain to localhost
masterFile.AddIPAddressResourceRecord("google.com", "127.0.0.1");
masterFile.AddIPAddressResourceRecord("github.com", "127.0.0.1");

// Log every request
server.Requested += (sender, e) => Console.WriteLine(e.Request);
// On every successful request log the request and the response
server.Responded += (sender, e) => Console.WriteLine("{0} => {1}", e.Request, e.Response);
// Log errors
server.Errored += (sender, e) => Console.WriteLine(e.Exception.Message);

// Start the server (by default it listens on port 53)
await server.Listen();
```

### 与[旧版源码](https://github.com/AigioL/dns)对比

#### Protocol 层主要变更

1. API 从 `byte[]` 序列化/反序列化为主，调整为 `ReadOnlyMemory<byte>` + `Write(Span<byte>)`。
   - 新版统计：`ReadOnlyMemory<byte>` 使用 41 处、`Write(Span<byte>)` 使用 10 处。
   - 旧版统计：上述两类使用均为 0。
2. `ToArray()` 由大量公开实例方法改为更少暴露。
   - 旧版约 11 处 `ToArray()` 方法；新版仅保留 1 处。
3. 端序抽象从旧版 `EndianAttribute` 迁移为 `IEndian`。
   - 删除：`Marshalling/EndianAttribute.cs`
   - 新增：`Marshalling/IEndian.cs`
   - `Struct` 相关逻辑已改造为 `StructHelper` + Span 路径。
4. 工程结构与文件变更。
   - 新增：`Kapetan.DNS.Protocol.shproj`、`Kapetan.DNS.Protocol.projitems`、`LICENSE`、本 README。
	 - 删除：`Utils/TaskExtensions.cs`、`Utils/ByteStream.cs`（相对旧版 Protocol 目录）。
5. 响应码类型从旧版自定义 `ResponseCode` 迁移为 .NET 11 的 `System.Net.DnsResponseCode`。
   - 旧文件 `ResponseCode.cs` 已移除，相关代码改为使用框架类型。

#### 内部实现变更

1. 减少 `byte[]` 频繁创建，降低内存碎片化风险。
   - 多处 `new byte[...]` 改为 `GC.AllocateUninitializedArray<byte>(...)`，并配合“后续完整覆盖写入”，避免不必要的零填充成本。
   - 在报文解析/序列化高频路径中，减少短生命周期数组分配，降低 GC 压力和 Gen0 抖动。

2. 引入“栈优先 + 池化兜底”的缓冲区策略。
   - `StructHelper.GetStruct<T>(ReadOnlySpan<byte>)` 先尝试 `stackalloc`（阈值 `StackallocByteThreshold = 256`），小结构体走栈内存，避免堆分配。
   - 超过阈值时再使用 `ArrayPool<byte>.Shared.Rent(...)`，复用大块缓冲，减少大数组反复申请与回收。

3. 结构体反序列化从 `Marshal.PtrToStructure` 迁移为 `MemoryMarshal.AsRef`。
   - 新流程：`ReadOnlySpan<byte>` -> 拷贝到可写 `Span<byte>` -> 端序调整 -> `MemoryMarshal.AsRef<T>(buffer)` 直接按内存布局解释 -> 值复制返回。
   - 不再依赖 `GCHandle` 固定对象与 `PtrToStructure`，降低 pinning 相关开销与互操作路径复杂度。

4. 写路径统一为 `Write(Span<byte>)`。
   - 由调用方一次性分配目标缓冲区，各层对象按切片写入，减少“每层先 `ToArray()` 再拼接”的中间数组。
   - 对长链路报文（Header/Question/Record）可显著减少临时对象数量。

5. 综合收益。
   - 更低的分配频率：减少短命 `byte[]`，降低 GC 扫描与回收成本。
   - 更低的碎片化概率：避免大量小数组在高并发下频繁进出托管堆。
   - 更好的吞吐稳定性：解析与序列化路径更线性，尾延迟更可控。
   - 保持协议正确性：端序转换仍由 `IEndian` + `ConvertEndian` 统一处理，兼容 DNS 网络字节序（大端）。

6. 反射与裁剪兼容性优化（`DynamicallyAccessedMembers`）。
   - `StructHelper` 的泛型参数增加了 `[DynamicallyAccessedMembers(...)]` 标注，明确运行时反射所需成员（构造函数/字段）。
   - 在 AOT/Trimming 场景中可降低“反射元数据被裁剪”导致的运行时失败风险，提高发布可靠性。

7. 正则表达式源生成优化（`[GeneratedRegex]`）。
   - `TextResourceRecord` 中的 TXT 记录解析正则改为源生成方式，避免每次运行时动态解析/编译正则。
   - 带来更低的启动开销与更稳定的匹配性能，同时减少临时分配。

#### Tests 层主要变更

1. 测试工程从 `Tests.csproj` 迁移为共享工程：`Kapetan.DNS.Tests.shproj` + `Kapetan.DNS.Tests.projitems`。
2. 针对新 API 增加测试辅助扩展：`Helper.cs` 中新增 `ToArray(this IMessage/IMessageEntry/Header/CharacterString)`，内部统一走 `Write(Span<byte>)`。
3. 新增 `Domain.cs`（测试目录根）。
4. Protocol 相关测试文件大多已同步修改（解析/序列化用例仍在），以适配新的 API 与断言行为。

#### 兼容性说明（可能导致旧测试失败）

- 依赖旧 `byte[]` 签名或实例 `ToArray()` 的调用/测试需要迁移到 `ReadOnlyMemory<byte>` 与 `Write(Span<byte>)`。

Depending on the application setup the events might be executed on a different thread than the calling thread.

It's also possible to modify the `request` instance in the `server.Requested` callback.

### Request Resolver

The `DnsServer`, `DnsClient` and `ClientRequest` classes also accept an instance implementing the `IRequestResolver` interface, which they internally use to resolve DNS requests. Some of the default implementations are `UdpRequestResolver`, `TcpRequestResolver` and `MasterFile` classes. But it's also possible to provide a custom request resolver.

```C#
// A request resolver that resolves all dns queries to localhost
public class LocalRequestResolver : IRequestResolver {
	public Task<IResponse> Resolve(IRequest request) {
		IResponse response = Response.FromRequest(request);

		foreach (Question question in response.Questions) {
			if (question.Type == RecordType.A) {
				IResourceRecord record = new IPAddressResourceRecord(
					question.Name, IPAddress.Parse("127.0.0.1"));
				response.AnswerRecords.Add(record);
			}
		}

		return Task.FromResult(response);
	}
}

// All dns requests received will be handled by the localhost request resolver
DnsServer server = new DnsServer(new LocalRequestResolver());

await server.Listen();
```
