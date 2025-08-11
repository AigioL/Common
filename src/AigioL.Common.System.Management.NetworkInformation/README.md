### AigioL.Common.System.Management.NetworkInformation
提供网络适配器（网卡）相关的工具类，使用 Windows Management Instrumentation (WMI) 实现。  
使用 ```WmiLight``` 包替代 ```System.Management``` 截止当前 .NET 9.0 需要内置的 COM 支持与 AOT 不兼容的问题。