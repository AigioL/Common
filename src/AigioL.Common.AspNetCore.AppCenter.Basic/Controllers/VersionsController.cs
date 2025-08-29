using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class VersionsController
{
    public static void MapBasicVersions(
        this IEndpointRouteBuilder b,
        [StringSyntax("Route")] string pattern = "basic/versions")
    {
        var routeGroup = b.MapGroup(pattern)
            .AllowAnonymous();

        routeGroup.MapGet("f3766643/{target}/{arch}/{current_version}", async (HttpContext context,
            [FromRoute] string target,
            [FromRoute] string arch,
            [FromRoute] string current_version) =>
        {
            if (IsVersionString(current_version))
            {
                var clientPlatform = Tauri.GetClientPlatform(target, arch);
                if (clientPlatform.HasValue)
                {
                    var r = await GetLatestVersionAsync(
                        default!,
                        clientPlatform.Value,
                        null,
                        DeploymentMode.SCD,
                        false);
                    if (r != null)
                    {
                        var r2 = r.ToTauri();
                        return Results.Json(r2,
                            AppVersionTauriModelJsonSerializerContext.Default.AppVersionTauriModel);
                    }
                }
            }
            return Results.NoContent(); // 如果无可用更新，你的服务器应响应状态代码 204 无内容。
        }).WithDescription(
"""
检查更新（Tauri）
https://tauri.org.cn/v1/guides/distribution/updater
""")
            .Produces<AppVersionTauriModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

    }

    /// <summary>
    /// 版本号格式校验
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsVersionString(ReadOnlySpan<char> s)
    {
        if (s.Length != 0 && s.Length < MaxLengths.Max_Version)
        {
            if (char.ToLowerInvariant(s[0]) == 'v')
            {
                s = s[1..];
            }
            var hasDot = false;
            for (int i = 0; i < s.Length; i++)
            {
                var it = s[i];
                if (!char.IsDigit(it))
                {
                    if (it == '.')
                    {
                        hasDot = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return hasDot;
        }
        return false;
    }

    /// <summary>
    /// 获取最新版本
    /// </summary>
    /// <param name="appVersion">当前应用版本信息</param>
    /// <param name="platform">(单选)客户端平台</param>
    /// <param name="osVersion">当前设备运行的操作系统版本号</param>
    /// <param name="deploymentMode">应用部署模式</param>
    /// <param name="includeBeta">是否接受 Beta 版本</param>
    static async Task<AppVersionModel?> GetLatestVersionAsync(
        string appVersion,
        ClientPlatform platform,
        Version? osVersion,
        DeploymentMode deploymentMode,
        bool includeBeta)
    {
        // TODO: 实现获取最新版本逻辑
        throw new NotImplementedException();
        await Task.CompletedTask;
    }
}

file static partial class Tauri
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static DevicePlatform2? GetDevicePlatform2(string target)
    {
        if (string.Equals("windows", target, StringComparison.InvariantCultureIgnoreCase))
        {
            return DevicePlatform2.Windows;
        }
        else if (string.Equals("darwin", target, StringComparison.InvariantCultureIgnoreCase))
        {
            return DevicePlatform2.macOS;
        }
        else if (string.Equals("linux", target, StringComparison.InvariantCultureIgnoreCase))
        {
            return DevicePlatform2.Linux;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Architecture? GetArchitecture(string arch)
    {
        if (string.Equals("x86_64", arch, StringComparison.InvariantCultureIgnoreCase))
        {
            return Architecture.X64;
        }
        else if (string.Equals("i686", arch, StringComparison.InvariantCultureIgnoreCase))
        {
            return Architecture.X86;
        }
        else if (string.Equals("aarch64", arch, StringComparison.InvariantCultureIgnoreCase))
        {
            return Architecture.Arm64;
        }
        else if (string.Equals("armv7", arch, StringComparison.InvariantCultureIgnoreCase))
        {
            return Architecture.Arm;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ClientPlatform? GetClientPlatform(string target, string arch)
    {
        var devicePlatform2 = GetDevicePlatform2(target);
        if (!devicePlatform2.HasValue)
        {
            return null;
        }
        var architecture = GetArchitecture(arch);
        if (!architecture.HasValue)
        {
            return null;
        }

        var r = devicePlatform2.Value.GetClientPlatform(architecture.Value);
        return r;
    }

    internal static AppVersionTauriModel ToTauri(this AppVersionModel m)
    {
        throw new NotImplementedException("TODO: ");
        return null;
    }
}