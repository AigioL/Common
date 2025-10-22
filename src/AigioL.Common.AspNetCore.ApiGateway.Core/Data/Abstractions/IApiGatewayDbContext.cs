using AigioL.Common.AspNetCore.ApiGateway.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using static AigioL.Common.AspNetCore.Helpers.ProgramMain.ProgramHelper;

namespace AigioL.Common.AspNetCore.ApiGateway.Data.Abstractions;

/// <summary>
/// Api 网关服务的数据库上下文接口
/// </summary>
public partial interface IApiGatewayDbContext : IDbContext
{
    /// <inheritdoc cref="YarpReverseProxyConfig"/>
    DbSet<YarpReverseProxyConfig> YarpReverseProxyConfigs { get; }

    /// <inheritdoc cref="ServerCertificate"/>
    DbSet<ServerCertificate> ServerCertificates { get; }

    /// <inheritdoc cref="WebRootPath"/>
    DbSet<WebRootPath> WebRootPaths { get; }
}

partial interface IApiGatewayDbContext
{
    /// <inheritdoc cref="DbContext.Database"/>
    DatabaseFacade Database => GetDbContext().Database;

    /// <inheritdoc cref="DbContext.SaveChangesAsync(CancellationToken)"/>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken) => GetDbContext().SaveChangesAsync(cancellationToken);

    /// <inheritdoc cref="DbContext.SaveChangesAsync(bool, CancellationToken)"/>
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken) => GetDbContext().SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
}