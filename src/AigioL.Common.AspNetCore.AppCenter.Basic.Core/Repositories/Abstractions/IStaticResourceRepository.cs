using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IStaticResourceRepository : IRepository<StaticResource, Guid>, IEFRepository
{
}

partial interface IStaticResourceRepository // 微服务
{

}