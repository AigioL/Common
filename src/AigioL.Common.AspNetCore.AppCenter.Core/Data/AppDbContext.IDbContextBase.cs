using AigioL.Common.AspNetCore.Helpers.ProgramMain;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data;

partial class AppDbContext : ProgramHelper.IDbContext
{
    /// <inheritdoc/>
    DbContext ProgramHelper.IDbContext.GetDbContext() => this;
}

partial class AppDbContext : IDbContextBase
{
    /// <inheritdoc/>
    DbContext IDbContextBase.GetDbContext() => this;

    /// <inheritdoc/>
    DatabaseFacade IDbContextBase.GetDatabase() => Database;
}
