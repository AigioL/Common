using AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Services.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using AigioL.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Services;

/// <summary>
/// PC 用户配置服务实现
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public sealed partial class PCUserConfigService<TDbContext> : IPCUserConfigService
    where TDbContext : DbContext, IPCDbContext2
{
    readonly TDbContext db;
    readonly ILogger<PCUserConfigService<TDbContext>> logger;

    /// <summary>
    /// <inheritdoc cref="PCUserConfigService{TDbContext}"/>
    /// </summary>
    /// <param name="db"></param>
    /// <param name="logger"></param>
    public PCUserConfigService(
        TDbContext db,
        ILogger<PCUserConfigService<TDbContext>> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PCUserConfig> GetConfigAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var config = await db.Set<PCUserConfig>()
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == userId, cancellationToken);

        if (config != null)
        {
            return config;
        }

        config = new PCUserConfig
        {
            Id = userId,
        };
        db.Set<PCUserConfig>().Add(config);
        await db.SaveChangesAsync(cancellationToken);

        return config;
    }

    /// <inheritdoc/>
    public async Task<ApiRsp> BindWeChatOpenIdAsync(
        Guid userId,
        string openId,
        string realName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(openId))
        {
            return ApiRsp.Fail("微信 OpenId 不能为空");
        }
        if (string.IsNullOrWhiteSpace(realName))
        {
            return ApiRsp.Fail("真实姓名不能为空");
        }

        // 确保配置存在（不存在则自动创建）
        await GetConfigAsync(userId, cancellationToken);

        // 仅当 OpenId 为空时才允许绑定（ExecuteUpdate 绕过变更追踪，不考虑并发）
        var rowsAffected = await db.Set<PCUserConfig>()
            .Where(c => c.Id == userId && (c.OpenId == null || c.OpenId == ""))
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.OpenId, c => openId)
                .SetProperty(c => c.RealName, c => realName)
                .SetProperty(c => c.UpdateTime, c => DateTimeOffset.Now),
            cancellationToken);

        if (rowsAffected == 0)
        {
            logger.LogWarning("PC 用户 {UserId} 已绑定微信 OpenId，不允许重复绑定", userId);
            return ApiRsp.Fail("该用户已绑定微信 OpenId，不允许重复绑定");
        }

        logger.LogInformation("PC 用户 {UserId} 绑定微信 OpenId 成功", userId);
        return new ApiRsp { Code = unchecked((uint)HttpStatusCode.OK) };
    }

    /// <inheritdoc/>
    public async Task<ApiRsp> ChangeWeChatOpenIdAsync(
        Guid userId,
        string openId,
        string realName,
        Guid? operatorUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(openId))
        {
            return ApiRsp.Fail("微信 OpenId 不能为空");
        }
        if (string.IsNullOrWhiteSpace(realName))
        {
            return ApiRsp.Fail("真实姓名不能为空");
        }

        // 确保配置存在（不存在则自动创建）
        await GetConfigAsync(userId, cancellationToken);

        // 使用 ExecuteUpdate 直接更新，绕过变更追踪
        await db.Set<PCUserConfig>()
            .Where(c => c.Id == userId)
            .ExecuteUpdateAsync(s =>
            {
                s.SetProperty(c => c.OpenId, c => openId)
                 .SetProperty(c => c.RealName, c => realName)
                 .SetProperty(c => c.UpdateTime, c => DateTimeOffset.Now);

                if (operatorUserId.HasValue)
                {
                    s.SetProperty(c => c.OperatorUserId, c => operatorUserId.Value);
                }
            },
            cancellationToken);

        logger.LogInformation("PC 用户 {UserId} 更换微信 OpenId 与真实姓名成功", userId);
        return new ApiRsp { Code = unchecked((uint)HttpStatusCode.OK) };
    }
}
