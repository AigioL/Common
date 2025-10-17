using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Controllers;

public static partial class VersionsController
{
    /// <summary>
    /// 客户端版本数据内存中缓存过期时间 1 分钟
    /// </summary>
    const int versions_memory_timeout_minutes = 1;

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
                    var r = await CheckUpdate3_2(
                        context,
                        current_version,
                        clientPlatform.Value,
                        0, 0, 0,
                        DeploymentMode.SCD,
                        false);
                    if (r.Content != null)
                    {
                        var r2 = r.Content.ToTauri();
                        if (r2 != null)
                        {
                            return Results.Json(r2,
                                AppVersionTauriModelJsonSerializerContext.Default.AppVersionTauriModel);
                        }
                    }
                }
            }
            return Results.NoContent(); // 如果无可用更新，你的服务器应响应状态代码 204 无内容。
        })
            .WithDescription(
"""
检查更新（Tauri）
https://tauri.org.cn/v1/guides/distribution/updater
""")
            .Produces<AppVersionTauriModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        routeGroup.MapGet("{platform}/{osVersionMajor}/{osVersionMinor}/{osVersionBuild}/{deploymentMode=0}", async (HttpContext context,
            [FromRoute] ClientPlatform platform,
            [FromRoute] int osVersionMajor,
            [FromRoute] int osVersionMinor,
            [FromRoute] int osVersionBuild,
            [FromRoute] DeploymentMode deploymentMode,
            [FromQuery] bool includeBeta = false) =>
        {
            var r = await CheckUpdate3_2(
                context,
                null,
                platform,
                osVersionMajor,
                osVersionMinor,
                osVersionBuild,
                deploymentMode,
                includeBeta
                );
            return r;
        })
        .WithDescription("检查更新 v3.2");
    }

    /// <summary>
    /// 版本号格式校验
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsVersionString(ReadOnlySpan<char> s)
    {
        if (s.Length != 0 && s.Length < MaxLengths.Version)
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
    /// 检查更新 v3.2
    /// </summary>
    /// <param name="context"></param>
    /// <param name="appVersion"></param>
    /// <param name="platform"></param>
    /// <param name="osVersionMajor"></param>
    /// <param name="osVersionMinor"></param>
    /// <param name="osVersionBuild"></param>
    /// <param name="deploymentMode"></param>
    /// <param name="includeBeta"></param>
    /// <returns></returns>
    static async Task<ApiRsp<AppVersionModel?>> CheckUpdate3_2(
        HttpContext context,
        string? appVersion,
        ClientPlatform platform,
        int osVersionMajor,
        int osVersionMinor,
        int osVersionBuild,
        DeploymentMode deploymentMode,
        bool includeBeta)
    {
        if (!Enum.IsDefined(platform))
            return "Unknown device platform"; // 客户端平台必须是单选值
        if (osVersionMajor < 0 || osVersionMinor < 0)
            return "Unknown OSVersion"; // 操作系统版本必须是非负整数

        IReadOnlyAppVer? appVer;
        if (appVersion == null)
        {
            appVer = await context.GetAppVerAsync();
        }
        else
        {
            var appVerCoreService = context.RequestServices.GetRequiredService<IAppVerCoreService>();
            appVer = await appVerCoreService.GetAsync(appVersion);
        }

        if (appVer == null)
        {
            return HttpStatusCode.NoContent;
        }

        var cacheKey = $"{nameof(VersionsController)}_CheckUpdateV3.1_{appVer.Version}_{platform}_{osVersionMajor}{osVersionMinor}{osVersionBuild}_{deploymentMode}";
        var cache = context.RequestServices.GetRequiredService<IDistributedCache>();
        var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(versions_memory_timeout_minutes);

            var osVersion = osVersionMajor == 0 && osVersionMinor == 0 && osVersionBuild == 0 ? null :
                (osVersionBuild < 0 ?
                    new Version(osVersionMajor, osVersionMinor) :
                    new Version(osVersionMajor, osVersionMinor, osVersionBuild));

            var repo = context.RequestServices.GetRequiredService<IAppVerBuildRepository>();

            var latestVersion = await repo.GetLatestVersionAsync(appVer, platform, osVersion, deploymentMode, includeBeta);
            if (latestVersion != null)
            {
                if (latestVersion.Version == appVer.Version)
                {
                    // 版本号相同返回空值
                    return null;
                }
                try
                {
                    // 使用的版本号大于等于最新版本号，返回空值
                    if (new Version(appVer.Version) >= new Version(latestVersion.Version))
                    {
                        return null;
                    }
                }
                catch
                {
                }
            }

            return latestVersion;
        });
        return result;
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

    internal static AppVersionTauriModel? ToTauri(this AppVersionModel m, string? signature = null)
    {
        if (m.HasValue())
        {
            var url = m.Downloads?.FirstOrDefault(static x => x.HasValue() && x.IncrementalUpdate == false)?.DownloadUrl;
            if (url != null)
            {
                return new()
                {
                    Url = url,
                    Version = m.Version,
                    PublishTime = m.PublishTime,
                    Signature = signature,
                    Notes = m.ReleaseNote,
                };
            }
        }

        return null;
    }
}