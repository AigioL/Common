using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories;

sealed partial class StaticResourceRepository<TDbContext> :
    Repository<TDbContext, StaticResource, Guid>,
    IStaticResourceRepository where TDbContext : DbContext
{
    public StaticResourceRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class StaticResourceRepository<TDbContext> // 微服务
{

}