// https://github.com/microsoft/AppModelSamples/tree/master/Samples/PackageWithExternalLocation/cs

using System.Diagnostics;
using System.Runtime.Versioning;
using Windows.Foundation;
using Windows.Management.Deployment;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Windows.Management.Deployment;

/// <summary>
/// 稀疏包助手类
/// <para>仅当桌面应用在运行时具有包标识时，桌面应用才能使用许多 Windows 功能</para>
/// </summary>
[SupportedOSPlatform("Windows10.0.19041.0")]
public static class SparsePackageHelper
{
    /// <summary>
    /// 当前运行的进程是否有包标识
    /// </summary>
    /// <returns></returns>
    public static bool IsRunningWithIdentity()
    {
        uint length = 0;
        Span<char> chars = stackalloc char[1024];
        var result = PInvoke.GetCurrentPackageFullName(ref length, chars);
        return result == WIN32_ERROR.ERROR_SUCCESS;
    }

    /// <summary>
    /// 注册含有外部位置的稀疏包，注册成功后重新启动进程将带有包标识，仅返回结果
    /// </summary>
    /// <param name="externalLocation"></param>
    /// <param name="packagePath"></param>
    /// <returns></returns>
    public static async Task<RegisterPackageResult> RegisterPackageWithExternalLocationExAsync(string externalLocation, string packagePath)
    {
        var deploymentOperation = RegisterPackageWithExternalLocationAsync(externalLocation, packagePath);
        await deploymentOperation;
        var result = GetRegisterPackageResult(deploymentOperation);
        return result;
    }

    /// <summary>
    /// 注册含有外部位置的稀疏包，注册成功后重新启动进程将带有包标识，返回异步操作以及进度报告
    /// </summary>
    /// <param name="externalLocation"></param>
    /// <param name="packagePath"></param>
    /// <returns></returns>
    public static IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> RegisterPackageWithExternalLocationAsync(string externalLocation, string packagePath)
    {
        var externalUri = new Uri(externalLocation, UriKind.Absolute);
        var packageUri = new Uri(packagePath, UriKind.Absolute);

        //Console.WriteLine("exe Location {0}", externalLocation);
        //Console.WriteLine("msix Address {0}", packagePath);

        //Console.WriteLine("  exe Uri {0}", externalUri);
        //Console.WriteLine("  msix Uri {0}", packageUri);

        var packageManager = new PackageManager();

        //Declare use of an external location
        var options = new AddPackageOptions
        {
            ExternalLocationUri = externalUri,
        };

        //Console.WriteLine("Installing package {0}", packagePath);
        Debug.WriteLine("Waiting for package registration to complete...");

        var deploymentOperation = packageManager.AddPackageByUriAsync(packageUri, options);
        return deploymentOperation;
    }

    public enum RegisterPackageResult : byte
    {
        Success,
        Canceled,
        Error,
        Unknown,
    }

    public static RegisterPackageResult GetRegisterPackageResult(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> deploymentOperation)
    {
        if (deploymentOperation.Status == AsyncStatus.Error)
        {
            DeploymentResult deploymentResult = deploymentOperation.GetResults();
            Debug.WriteLine("Installation Error: {0}", deploymentOperation.ErrorCode);
            Debug.WriteLine("Detailed Error Text: {0}", deploymentResult.ErrorText);
            return RegisterPackageResult.Error;
        }
        else if (deploymentOperation.Status == AsyncStatus.Canceled)
        {
            Debug.WriteLine("Package Registration Canceled");
            return RegisterPackageResult.Canceled;
        }
        else if (deploymentOperation.Status == AsyncStatus.Completed)
        {
            Debug.WriteLine("Package Registration succeeded!");
            return RegisterPackageResult.Success;
        }
        else
        {
            Debug.WriteLine("Installation status unknown");
            return RegisterPackageResult.Unknown;
        }
    }

    public static async void RemovePackageWithExternalLocation(string packageFullName) => await RemovePackageWithExternalLocationAsync(packageFullName);

    public static IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> RemovePackageWithExternalLocationAsync(string packageFullName)
    {
        var packageManager = new PackageManager();

        Debug.WriteLine("Uninstalling package..");
        return packageManager.RemovePackageAsync(packageFullName);
    }
}