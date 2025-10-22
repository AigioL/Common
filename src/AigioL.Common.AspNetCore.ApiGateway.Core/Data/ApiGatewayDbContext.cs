using AigioL.Common.AspNetCore.ApiGateway.Data.Abstractions;
using AigioL.Common.AspNetCore.ApiGateway.Entities;
using AigioL.Common.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using static AigioL.Common.AspNetCore.Helpers.ProgramMain.ProgramHelper;

namespace AigioL.Common.AspNetCore.ApiGateway.Data;

/// <summary>
/// Api 网关数据库上下文
/// </summary>
/// <param name="httpContextAccessor"></param>
/// <param name="options"></param>
public sealed class ApiGatewayDbContext(
    DbContextOptions<ApiGatewayDbContext> options) :
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
    DbContext(options), IApiGatewayDbContext
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
{
    /// <inheritdoc/>
    DbContext IDbContext.GetDbContext() => this;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        _ = builder.BuildEntities();
    }

    /// <inheritdoc cref="YarpReverseProxyConfig"/>
    public DbSet<YarpReverseProxyConfig> YarpReverseProxyConfigs { get; set; } = null!;

    /// <inheritdoc cref="ServerCertificate" />
    public DbSet<ServerCertificate> ServerCertificates { get; set; } = null!;

    /// <inheritdoc cref="WebRootPath" />
    public DbSet<WebRootPath> WebRootPaths { get; set; } = null!;
}
