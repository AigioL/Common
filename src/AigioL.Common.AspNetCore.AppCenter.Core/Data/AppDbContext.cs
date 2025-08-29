#if !PROJ_DBCONTEXT_BM
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data;

public sealed partial class AppDbContext(
    IServiceProvider serviceProvider,
    DbContextOptions<AppDbContext> options) :
    AppDbContextBase(serviceProvider, options), IAppDbContextBase, IIdentityDbContext
{
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // 重命名 Identity 相关表名
        IAppDbContextBase.ToIdentitysTable(b);
    }
}
#endif