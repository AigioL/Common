# Steam Achievement Manager

Steam Achievement Manager (SAM) is a lightweight, portable application used to manage achievements and statistics in the popular PC gaming platform Steam. This application requires the [Steam client](https://store.steampowered.com/about/), a Steam account and network access. Steam must be running and the user must be logged in.

This is the code for SAM. The closed-source version originally released in 2008, last major release in 2011, and last updated in 2013 (a hotfix).

The code is being made available so that those interested can do as they like with it.

There are some changes to the code since the last closed-source release:
- General code maintenance to bring it into a more modern state.
- Icons have been replaced with ones from the Fugue Icons set.
- Version has been bumped to 7.0.x.x to indicate the open-source release.

[Download latest release](https://github.com/gibbed/SteamAchievementManager/releases/latest).

[![Build status](https://ci.appveyor.com/api/projects/status/00vic6jliar6j0ol/branch/master?svg=true)](https://ci.appveyor.com/project/gibbed/steamachievementmanager/branch/master)

## Attribution

Most (if not all) icons are from the [Fugue Icons](https://p.yusukekamiyamane.com/) set.

## 与[旧版源码](https://github.com/AigioL/SteamAchievementManager)对比

### 关键实现差异

1. 平台加载逻辑与 Steam 库绑定方式重构（`Steam.cs`）
   - 旧版偏 Windows 专用：`LoadLibraryEx` / `GetProcAddress` / `SetDllDirectory`。
   - 当前版本改为 `NativeLibrary.Load` / `NativeLibrary.GetExport`，并显式区分 Windows、macOS、Linux 路径。
   - 安装路径读取从 `HKLM\Software\Valve\Steam` 调整为 `HKCU\SOFTWARE\Valve\Steam`，并增加 `GetInstallPathDelegate` 便于注入自定义查找逻辑。
   - 改动收益：跨平台路径与装载流程更统一，便于在容器/CI 中运行与测试，减少平台分支带来的维护成本。

2. Native 调用模型从“反射委托缓存”迁移到“函数指针直调”（`NativeWrapper.cs` + 各 Wrappers）
   - 旧版通过 `Marshal.PtrToStructure` + `Marshal.GetDelegateForFunctionPointer` + `DynamicInvoke`。
   - 当前版本在包装层大量使用 `delegate* unmanaged[...]` 直接调用虚表函数。
   - 接口结构（如 `ISteam*`）普遍改为 `struct + nint` 函数槽形式，替代旧版 `class + IntPtr`。
   - 改动收益：显著减少运行时反射路径（`DynamicInvoke`/委托反射创建），对 NativeAOT 更友好，也更容易兼容 Trimming（裁剪）场景。

3. 客户端初始化的接口版本组合发生变化（`Client.cs`、`Wrappers/SteamClient018.cs`）
   - `SteamUser`：`012` -> `017`
   - `SteamUserStats`：`013` -> `011`（对应新增 `ISteamUserStats007`/`SteamUserStats007.cs` 实现）
   - `SteamUtils`：`005` -> `007`
   - 新增并接入：`SteamRemoteStorage012`、`SteamInventory002`
   - 当前版本在属性暴露上从公开字段转向属性封装，并加入空值检查。
   - 改动收益：接口版本管理更集中，初始化阶段失败点更明确，便于问题定位与后续扩展。

4. 字符串与缓冲区处理路径调整（`Helpers.cs`、`NativeStrings.cs`、`Wrappers/*`）
   - 新增 `Helpers`，引入 `StackallocByteThreshold` 与 `ArrayPool<byte>` 的分配策略。
   - `NativeStrings` 从“字符串分配/释放 + 多重 PointerToString 重载”收敛为 `PointerToSpan`（字节视图）。
   - 多处 Wrapper 改为先拿 `ReadOnlySpan<byte>`，再统一用 `Encoding.UTF8.GetString(...)` 解码。
   - 改动收益：减少中间字符串与临时数组创建，降低不必要的内存拷贝与 GC 压力；栈分配 + 池化策略在高频调用下更稳。

5. 功能面扩展与接口面收敛并存
   - 功能扩展：新增 Inventory / RemoteStorage / UGC 相关接口、类型与包装器。
   - 接口收敛：移除 `ISteamUserStats013` 与其包装器后，旧版中对应的较新统计接口槽位（如 Global Stats 一组能力）不再以 `013` 结构呈现。
   - 改动收益：扩展能力（道具/云存储/UGC）可直接复用；同时接口面更聚焦，减少历史层叠接口带来的歧义。

### 兼容性提示

- 如果上层代码依赖旧版 `SteamUserStats013`/`CallHandle`/`NativeStrings.StringHandle`，迁移到当前源码时需要重写对应调用。
- 当前代码在 Steam 接口版本号、native 调用约定、字符串/内存处理上均有系统性变化，不建议仅做文件替换式升级。
