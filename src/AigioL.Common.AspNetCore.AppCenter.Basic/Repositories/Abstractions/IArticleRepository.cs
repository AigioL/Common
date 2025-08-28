using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IArticleRepository : IRepository<Article, Guid>, IEFRepository
{
}

partial interface IArticleRepository // 管理后台
{

}

partial interface IArticleRepository // 微服务
{
}