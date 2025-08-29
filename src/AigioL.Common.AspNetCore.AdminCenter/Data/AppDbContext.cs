#if PROJ_DBCONTEXT_BM
using AigioL.Common.AspNetCore.AdminCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AdminCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using Microsoft.EntityFrameworkCore;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data;

public sealed partial class AppDbContext(
    IServiceProvider serviceProvider,
    DbContextOptions<AppDbContext> options) :
    BMDbContextBase<BMUser, BMRole, BMUserRole>(serviceProvider, options)
{
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        IAppDbContextBase.OnModelCreatingVersion2(this, b);

        // 重命名 Identity 相关表名
        IAppDbContextBase.ToIdentitysTable(b);
    }
}
#endif