using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IAppVerBuildRepository : IRepository<AppVerBuild, Guid>, IEFRepository
{
}

partial interface IAppVerBuildRepository
{
    /// <summary>
    /// 获取最新版本
    /// </summary>
    /// <param name="appVer">当前应用版本信息</param>
    /// <param name="platform">(单选)客户端平台</param>
    /// <param name="osVersion">当前设备运行的操作系统版本号</param>
    /// <param name="deploymentMode">应用部署模式</param>
    /// <param name="includeBeta">是否接受 Beta 版本</param>
    Task<AppVersionModel?> GetLatestVersionAsync(
        IReadOnlyAppVer? appVer,
        ClientPlatform platform,
        Version? osVersion,
        DeploymentMode deploymentMode,
        bool includeBeta);
}