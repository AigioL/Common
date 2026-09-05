## [Protocol Buffers](https://protobuf.com.cn)
Protocol buffers 是 Google 开发的一种语言无关、平台无关、可扩展的序列化结构数据机制——类似于 XML，但体积更小、速度更快、更简单。您只需定义一次数据的结构化方式，然后就可以使用生成的特殊源代码，轻松地在各种语言中向各种数据流写入和读取结构化数据

在 C# 中使用 [protobuf-net](https://github.com/protobuf-net/protobuf-net) 库  

### 生成 C# 模型类
在需要引用的 .csproj 项目中末尾添加 ```<Import Project="$(MSBuildThisFileDirectory)..\AigioL.Common.Stm.Models.Shared\Protobuf\AdditionalFiles.props" />```，路径根据实际情况修改

### C# 命名空间
所有 .proto 文件应放入 Protos 文件夹中，需要在 .proto 文件中指定 C# 命名空间，例如：
```
// [START csharp_declaration]
option csharp_namespace = "AigioL.Common.Stm.Models.Protobuf";
// [END csharp_declaration]
```
除非 .proto 文件为通用的依赖项比如 ```google-protobuf-descriptor``` 等