using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.Models;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Services.Abstractions;

/// <summary>
/// PC 用户配置服务
/// </summary>
public partial interface IPCUserConfigService
{
    /// <summary>
    /// 获取 PC 用户配置，如果不存在则创建一个并返回
    /// </summary>
    /// <param name="userId">PC 用户 Id</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PCUserConfig> GetConfigAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 绑定微信 OpenId 与真实姓名，如果已经绑定则不允许再次绑定
    /// </summary>
    /// <param name="userId">PC 用户 Id</param>
    /// <param name="openId">微信 OpenId</param>
    /// <param name="realName">真实姓名</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp> BindWeChatOpenIdAsync(
        Guid userId,
        string openId,
        string realName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更换微信 OpenId 与真实姓名
    /// </summary>
    /// <param name="userId">PC 用户 Id</param>
    /// <param name="openId">新的微信 OpenId</param>
    /// <param name="realName">真实姓名</param>
    /// <param name="operatorUserId">操作人 Id（后台用户 Id），用于记录最后修改人</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ApiRsp> ChangeWeChatOpenIdAsync(
        Guid userId,
        string openId,
        string realName,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default);
}
