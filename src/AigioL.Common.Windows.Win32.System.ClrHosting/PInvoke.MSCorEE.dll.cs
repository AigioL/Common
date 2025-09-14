/* MSCorEE.dll 是 Microsoft .NET Framework 的核心系统组件，承担公共语言运行时（CLR）的初始化与托管功能。
 * 此 C# 代码仅在 CoreCLR/NativeAOT 中使用，在 .NET Framework 中不能重复操作 CLR！
 */
#pragma warning disable SYSLIB1054 // 使用 “LibraryImportAttribute” 而不是 “DllImportAttribute” 在编译时生成 P/Invoke 封送代码
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.System.ClrHosting;
using static Windows.Win32.System.ClrHosting.MetaHost;

namespace Windows.Win32
{
    static partial class PInvoke
    {
        /// <summary>
        /// 提供以下三个接口之一：ICLRMetaHost、ICLRMetaHostPolicy 或 ICLRDebugging。
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/clrcreateinstance-function</para>
        /// </summary>
        /// <param name="clsid">三个类标识符之一：CLSID_CLRMetaHost、CLSID_CLRMetaHostPolicy 或 CLSID_CLRDebugging。</param>
        /// <param name="riid">三个接口标识符 (IID) 之一：IID_ICLRMetaHost、IID_ICLRMetaHostPolicy 或 IID_ICLRDebugging。</param>
        /// <param name="ppInterface">三个接口之一：ICLRMetaHost、ICLRMetaHostPolicy 或 ICLRDebugging。</param>
        /// <returns>
        /// 此方法返回以下特定 HRESULT 以及表示方法失败的 HRESULT 错误。
        /// <list type="bullet">
        /// <item>S_OK	该方法已成功完成。</item>
        /// <item>E_POINTER	ppInterface 为 null。</item>
        /// </list>
        /// </returns>
        [DllImport("MSCorEE.dll", ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern unsafe int CLRCreateInstance(
            Guid* clsid,
            Guid* riid,
            void** ppInterface);

        /// <inheritdoc cref="CLRCreateInstance(Guid*, Guid*, void**)"/>
        internal static unsafe HRESULT CLRCreateInstance(
            in Guid clsid,
            in Guid riid,
            out void* ppInterface)
        {
            fixed (Guid* riidLocal = &riid)
            {
                fixed (Guid* clsidLocal = &clsid)
                {
                    fixed (void** ppInterfaceLocal = &ppInterface)
                    {
                        var __result = CLRCreateInstance(clsidLocal, riidLocal, ppInterfaceLocal);
                        return new(__result);
                    }
                }
            }
        }

        /// <inheritdoc cref="ICLRMetaHost.GetRuntime"/>
        internal static unsafe HRESULT GetRuntime(
            this ICLRMetaHost i,
            PCWSTR pwzVersion,
            in Guid riid,
            out void* ppRuntime)
        {
            int result;
            fixed (Guid* riidLocal = &riid)
            {
                fixed (void** ppRuntimeLocal = &ppRuntime)
                {
                    result = i.GetRuntime(pwzVersion.Value, riidLocal, ppRuntimeLocal);
                }
            }
            return new(result);
        }

        /// <inheritdoc cref="ICLRRuntimeInfo.GetInterface"/>
        internal static unsafe HRESULT GetInterface(
            this ICLRRuntimeInfo i,
            in Guid rclsid,
            in Guid riid,
            out void* ppUnk)
        {
            int result;
            fixed (Guid* rclsidLocal = &rclsid)
            {
                fixed (Guid* riidLocal = &riid)
                {
                    fixed (void** ppUnkLocal = &ppUnk)
                    {
                        result = i.GetInterface(rclsidLocal, riidLocal, ppUnkLocal);
                    }
                }
            }
            return new(result);
        }

        /// <inheritdoc cref="ICLRRuntimeHost.ExecuteInDefaultAppDomain"/>
        internal static unsafe HRESULT ExecuteInDefaultAppDomain(
            this ICLRRuntimeHost i,
            string pwzAssemblyPath,
            string pwzTypeName,
            string pwzMethodName,
            string? pwzArgument,
            out uint pReturnValue)
        {
            pwzArgument ??= string.Empty;
            int result;
            fixed (char* pwzAssemblyPathLocal = pwzAssemblyPath)
            {
                fixed (char* pwzTypeNameLocal = pwzTypeName)
                {
                    fixed (char* pwzMethodNameLocal = pwzMethodName)
                    {
                        fixed (char* pwzArgumentLocal = pwzArgument)
                        {
                            fixed (uint* pReturnValueLocal = &pReturnValue)
                            {
                                result = i.ExecuteInDefaultAppDomain(
                                    pwzAssemblyPathLocal,
                                    pwzTypeNameLocal,
                                    pwzMethodNameLocal,
                                    pwzArgumentLocal,
                                    pReturnValueLocal);
                            }
                        }
                    }
                }
            }
            return new(result);
        }

        static readonly Lazy<ClrHost> clrHostLazy = new(() => new ClrHost(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// 传递 .NET Framework 4.x 程序集路径、类型名称、方法名称和可选参数，以在当前进程中加载公共语言运行时 (CLR) 并调用托管代码。
        /// </summary>
        /// <param name="pwzAssemblyPath">指向 Assembly 的路径，定义了要调用其方法的 Type。</param>
        /// <param name="pwzTypeName">定义了 Type 的名称，该元素定义了要调用的方法（函数签名为【static int pwzMethodName (string pwzArgument);】）。</param>
        /// <param name="pwzMethodName">要调用的方法的名称。</param>
        /// <param name="pwzArgument">要传递给方法的字符串参数。</param>
        /// <returns>调用方法返回的整数值。</returns>
        public static unsafe uint RunDotNetFxDllMain(
            string pwzAssemblyPath,
            string pwzTypeName,
            string pwzMethodName,
            string? pwzArgument = null)
        {
            var pReturnValue = clrHostLazy.Value.ExecuteInDefaultAppDomain(pwzAssemblyPath, pwzTypeName, pwzMethodName, pwzArgument);
            return pReturnValue;
        }
    }

    namespace System.ClrHosting
    {
        /// <summary>
        /// 提供一些方法，这些方法基于公共语言运行时 (CLR) 的版本号返回特定版本的公共语言运行时，列出所有已安装的 CLR，列出在指定进程中加载的所有运行时，发现编译程序集所用的 CLR 版本，退出使用干净运行时关闭的进程，以及查询旧的 API 绑定。
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrmetahost-interface</para>
        /// </summary>
        [Guid("D332DB9E-B9B3-4125-8207-A14884F53216")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [GeneratedComInterface]
        internal unsafe partial interface ICLRMetaHost
        {
            /// <summary>
            /// 获取 ICLRRuntimeInfo 接口，该接口对应于公共语言运行时 (CLR) 的特定版本。 此方法取代了与 STARTUP_LOADER_SAFEMODE 标记一同使用的 CorBindToRuntimeEx 函数。
            /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrmetahost-getruntime-method</para>
            /// </summary>
            /// <param name="pwzVersion">存储在元数据中的 .NET Framework 编译版本，格式为“vA.B[.X]”。 A、B 和 X 是对应于主版本、次要版本和生成号的十进制数字。</param>
            /// <param name="riid">所需接口的标识符。 目前，此参数唯一的有效值为 IID_ICLRRuntimeInfo。</param>
            /// <param name="ppRuntime">指向与请求的运行时对应的 ICLRRuntimeInfo 接口的指针。</param>
            /// <returns>
            /// 此方法返回以下特定 HRESULT 以及表示方法失败的 HRESULT 错误。
            /// <list type="bullet">
            /// <item>S_OK	该方法已成功完成。</item>
            /// <item>E_POINTER	ppRuntime 为 null。</item>
            /// </list>
            /// </returns>
            int GetRuntime(char* pwzVersion, Guid* riid, void** ppRuntime);

            //[return: MarshalAs(UnmanagedType.IUnknown)]
            //unsafe object GetRuntime(PCWSTR pwzVersion, global::System.Guid* riid);

            //void GetVersionFromFile(PCWSTR pwzFilePath, PCWSTR pwzBuffer, ref uint pcchBuffer);

            //System.Com.IEnumUnknown EnumerateInstalledRuntimes();

            //System.Com.IEnumUnknown EnumerateLoadedRuntimes(HANDLE hndProcess);

            //void RequestRuntimeLoadedNotification([MarshalAs(UnmanagedType.FunctionPtr)] System.ClrHosting.RuntimeLoadedCallbackFnPtr pCallbackFunction);

            //[return: MarshalAs(UnmanagedType.IUnknown)]
            //unsafe object QueryLegacyV2RuntimeBinding(global::System.Guid* riid);

            //void ExitProcess(int iExitCode);
        }

        /// <summary>
        /// 提供返回有关特定公共语言运行时（CLR）的信息的方法，包括版本、目录和加载状态。 此接口还提供特定于运行时的功能，而无需初始化运行时。 它包括运行时相对 LoadLibrary 方法、特定于运行时模块的 GetProcAddress 方法和通过 GetInterface 方法提供的运行时提供的接口。
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrruntimeinfo-interface</para>
        /// </summary>
        [Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [GeneratedComInterface]
        internal unsafe partial interface ICLRRuntimeInfo
        {
            //void GetVersionString(winmdroot.Foundation.PWSTR pwzBuffer, ref uint pcchBuffer);

            //void GetRuntimeDirectory(winmdroot.Foundation.PWSTR pwzBuffer, ref uint pcchBuffer);

            //winmdroot.Foundation.BOOL IsLoaded(winmdroot.Foundation.HANDLE hndProcess);

            //void LoadErrorString(uint iResourceID, winmdroot.Foundation.PWSTR pwzBuffer, ref uint pcchBuffer, int iLocaleID);

            //winmdroot.Foundation.HMODULE LoadLibraryA(winmdroot.Foundation.PCWSTR pwzDllName);

            //unsafe void* GetProcAddress(winmdroot.Foundation.PCSTR pszProcName);

            /// <summary>
            /// 将 CLR 加载到当前进程，并返回运行时接口指针，例如 ICLRRuntimeHost、 ICLRStrongName 和 IMetaDataDispenserEx。
            /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrruntimeinfo-getinterface-method</para>
            /// </summary>
            /// <param name="rclsid">coclass 的 CLSID 接口。</param>
            /// <param name="riid">请求 rclsid 的接口的 IID。</param>
            /// <param name="ppUnk">指向查询接口的指针。</param>
            /// <returns>
            /// 此方法返回以下特定的 HRESULT 以及指示方法失败的 HRESULT 错误。
            /// <list type="bullet">
            /// <item>S_OK	该方法已成功完成。</item>
            /// <item>E_POINTER	ppUnk 为 null。</item>
            /// <item>E_OUTOFMEMORY	没有足够的内存可用于处理请求。</item>
            /// <item>CLR_E_SHIM_LEGACYRUNTIMEALREADYBOUND	其他运行时已绑定到旧版 CLR 版本 2 激活策略。</item>
            /// </list>
            /// </returns>
            int GetInterface(Guid* rclsid, Guid* riid, void** ppUnk);

            //[return: MarshalAs(UnmanagedType.IUnknown)]
            //unsafe object GetInterface(global::System.Guid* rclsid, global::System.Guid* riid);

            //winmdroot.Foundation.BOOL IsLoadable();

            //void SetDefaultStartupFlags(uint dwStartupFlags, winmdroot.Foundation.PCWSTR pwzHostConfigFile);

            //void GetDefaultStartupFlags(out uint pdwStartupFlags, winmdroot.Foundation.PWSTR pwzHostConfigFile, ref uint pcchHostConfigFile);

            //void BindAsLegacyV2Runtime();

            //unsafe void IsStarted(winmdroot.Foundation.BOOL* pbStarted, out uint pdwStartupFlags);
        }

        /// <summary>
        /// 提供的功能类似于 .NET Framework 版本 1 中提供的 ICorRuntimeHost 接口的功能，但有以下更改：
        /// <list type="bullet">
        /// <item>添加了 SetHostControl 方法来设置主机控件接口。</item>
        /// <item>省略了 ICorRuntimeHost 提供的一些方法。</item>
        /// </list>
        /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-interface</para>
        /// </summary>
        [Guid("90F1A06C-7712-4762-86B5-7A5EBA6BDB02")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [GeneratedComInterface]
        internal unsafe partial interface ICLRRuntimeHost
        {
            /// <summary>
            /// 将公共语言运行时 (CLR) 初始化到进程中。
            /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-start-method</para>
            /// </summary>
            /// <returns>
            /// 此方法返回以下特定的 HRESULT 以及指示方法失败的 HRESULT 错误。
            /// <list type="bullet">
            /// <item>S_OK	已成功返回 Start。</item>
            /// <item>HOST_E_CLRNOTAVAILABLE	CLR 未加载到进程中，或 CLR 处于无法运行托管代码或无法成功处理调用的状态。</item>
            /// <item>HOST_E_TIMEOUT	调用超时。</item>
            /// <item>HOST_E_NOT_OWNER	调用方未持有锁。</item>
            /// <item>HOST_E_ABANDONED	阻塞的线程或纤程正在等待某一事件，而该事件已被取消。</item>
            /// <item>E_FAIL	发生未知的灾难性故障。 如果方法返回 E_FAIL，则进程中无法再使用 CLR。 后续调用承载方法会返回 HOST_E_CLRNOTAVAILABLE。</item>
            /// </list>
            /// </returns>
            int Start();

            /// <summary>
            /// 通过公共语言运行时 (CLR) 停止代码执行。
            /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-stop-method</para>
            /// </summary>
            /// <returns>
            /// 此方法返回以下特定的 HRESULT 以及指示方法失败的 HRESULT 错误。
            /// <list type="bullet">
            /// <item>S_OK	已成功返回 Stop。</item>
            /// <item>HOST_E_CLRNOTAVAILABLE	CLR 未加载到进程中，或 CLR 处于无法运行托管代码或无法成功处理调用的状态。</item>
            /// <item>HOST_E_TIMEOUT	调用超时。</item>
            /// <item>HOST_E_NOT_OWNER	调用方未持有锁。</item>
            /// <item>HOST_E_ABANDONED	阻塞的线程或纤程正在等待某一事件，而该事件已被取消。</item>
            /// <item>E_FAIL	发生未知的灾难性故障。 如果方法返回 E_FAIL，则进程中无法再使用 CLR。 后续调用承载方法会返回 HOST_E_CLRNOTAVAILABLE。</item>
            /// </list>
            /// </returns>
            int Stop();

            //void SetHostControl(winmdroot.System.ClrHosting.IHostControl pHostControl);

            //void GetCLRControl(out winmdroot.System.ClrHosting.ICLRControl pCLRControl);

            //void UnloadAppDomain(uint dwAppDomainId, winmdroot.Foundation.BOOL fWaitUntilDone);

            //unsafe void ExecuteInAppDomain(uint dwAppDomainId, [MarshalAs(UnmanagedType.FunctionPtr)] winmdroot.System.ClrHosting.FExecuteInAppDomainCallback pCallback, void* cookie);

            //void GetCurrentAppDomainId(out uint pdwAppDomainId);

            //unsafe void ExecuteApplication(winmdroot.Foundation.PCWSTR pwzAppFullName, uint dwManifestPaths, winmdroot.Foundation.PCWSTR* ppwzManifestPaths, uint dwActivationData, winmdroot.Foundation.PCWSTR* ppwzActivationData, out int pReturnValue);

            //void ExecuteInDefaultAppDomain(winmdroot.Foundation.PCWSTR pwzAssemblyPath, winmdroot.Foundation.PCWSTR pwzTypeName, winmdroot.Foundation.PCWSTR pwzMethodName, winmdroot.Foundation.PCWSTR pwzArgument, out uint pReturnValue);

            /// <summary>
            /// 调用指定托管程序集中指定类型的指定方法。
            /// <para>https://learn.microsoft.com/zh-cn/dotnet/framework/unmanaged-api/hosting/iclrruntimehost-executeindefaultappdomain-method</para>
            /// </summary>
            /// <param name="pwzAssemblyPath">指向 Assembly 的路径，定义了要调用其方法的 Type。</param>
            /// <param name="pwzTypeName">定义了 Type 的名称，该元素定义了要调用的方法。</param>
            /// <param name="pwzMethodName">要调用的方法的名称。</param>
            /// <param name="pwzArgument">要传递给方法的字符串参数。</param>
            /// <param name="pReturnValue">调用方法返回的整数值。</param>
            /// <returns>
            /// 此方法返回以下特定的 HRESULT 以及指示方法失败的 HRESULT 错误。
            /// <list type="bullet">
            /// <item>S_OK	已成功返回 ExecuteInDefaultAppDomain。</item>
            /// <item>HOST_E_CLRNOTAVAILABLE	公共语言运行时 (CLR) 未加载到进程中，或 CLR 处于无法运行托管代码或无法成功处理调用的状态。</item>
            /// <item>HOST_E_TIMEOUT	调用超时。</item>
            /// <item>HOST_E_NOT_OWNER	调用方未持有锁。</item>
            /// <item>HOST_E_ABANDONED	阻塞的线程或纤程正在等待某一事件，而该事件已被取消。</item>
            /// <item>E_FAIL	发生未知的灾难性故障。 如果方法返回 E_FAIL，则进程中无法再使用 CLR。 后续调用承载方法会返回 HOST_E_CLRNOTAVAILABLE。</item>
            /// </list>
            /// </returns>
            int ExecuteInDefaultAppDomain(
                char* pwzAssemblyPath,
                char* pwzTypeName,
                char* pwzMethodName,
                char* pwzArgument,
                uint* pReturnValue);
        }

        /// <summary>
        /// MetaHost.h 常量定义
        /// </summary>
        internal static partial class MetaHost
        {
            internal static readonly Guid CLSID_CLRMetaHost = new("9280188d-0e8e-4867-b30c-7fa83884e8de");
            internal static readonly Guid IID_ICLRMetaHost = new("D332DB9E-B9B3-4125-8207-A14884F53216");
            internal static readonly Guid CLSID_CLRRuntimeHost = new("90F1A06E-7712-4762-86B5-7A5EBA6BDB02");
            internal static readonly Guid IID_ICLRRuntimeHost = new("90F1A06C-7712-4762-86B5-7A5EBA6BDB02");
        }

        /// <summary>
        /// .NET Framework 的 CLR 主机，负责加载和启动 CLR，并提供调用托管代码的方法。
        /// </summary>
        internal sealed unsafe class ClrHost : IDisposable
        {
            public ICLRRuntimeInfo RuntimeInfo { get; }

            public ICLRRuntimeHost RuntimeHost { get; }

            public HRESULT RESULT => hr;

            HRESULT hr;
            bool disposedValue;

            internal ClrHost(string pVersion = "v4.0.30319")
            {
                // 1. 创建 CLR MetaHost 实例
                hr = PInvoke.CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, out var ppInterface);
                hr.ThrowOnFailure();

                // 2. 将 COM 指针转换为托管接口
                ComWrappers cw = new StrategyBasedComWrappers();
                var pMetaHost = (ICLRMetaHost)cw.GetOrCreateObjectForComInstance(
                    (nint)ppInterface,
                    CreateObjectFlags.None);

                // 3. 获取指定版本的运行时
                void* ppRuntime;
                fixed (char* pVersionLocal = pVersion)
                {
                    hr = pMetaHost.GetRuntime(pVersionLocal, CLSID_CLRRuntimeHost, out ppRuntime);
                }
                hr.ThrowOnFailure();

                RuntimeInfo = (ICLRRuntimeInfo)cw.GetOrCreateObjectForComInstance(
                    (nint)ppRuntime,
                    CreateObjectFlags.None);

                // 4. 获取运行时主机接口
                hr = RuntimeInfo.GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, out var ppUnk);
                hr.ThrowOnFailure();

                RuntimeHost = (ICLRRuntimeHost)cw.GetOrCreateObjectForComInstance(
                    (nint)ppUnk,
                    CreateObjectFlags.None);

                // 5. 启动 CLR
                hr = new(RuntimeHost.Start());
                hr.ThrowOnFailure();
            }

            /// <summary>
            /// 调用指定托管程序集中指定类型的指定方法。
            /// </summary>
            /// <param name="pwzAssemblyPath">指向 Assembly 的路径，定义了要调用其方法的 Type。</param>
            /// <param name="pwzTypeName">定义了 Type 的名称，该元素定义了要调用的方法。</param>
            /// <param name="pwzMethodName">要调用的方法的名称。</param>
            /// <param name="pwzArgument">要传递给方法的字符串参数。</param>
            /// <returns>调用方法返回的整数值。</returns>
            public uint ExecuteInDefaultAppDomain(
                string pwzAssemblyPath,
                string pwzTypeName,
                string pwzMethodName,
                string? pwzArgument)
            {
                // 6. 执行托管代码
                hr = RuntimeHost.ExecuteInDefaultAppDomain(
                    pwzAssemblyPath, pwzTypeName, pwzMethodName,
                    pwzArgument, out var pReturnValue);
                hr.ThrowOnFailure();
                return pReturnValue;
            }

            public void StopClr(bool throwOnFailure = true)
            {
                // 7. 停止 CLR
                hr = new(RuntimeHost.Stop());
                if (throwOnFailure)
                {
                    hr.ThrowOnFailure();
                }
            }

            void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // 释放托管状态(托管对象)
                        StopClr(false);
                    }

                    // 释放未托管的资源(未托管的对象)并重写终结器
                    // 将大型字段设置为 null
                    disposedValue = true;
                }
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}