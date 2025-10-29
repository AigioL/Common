using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;

public interface IArticleDbContext : IDbContextBase
{
    DbSet<Article> Articles { get; }

    DbSet<ArticleCategory> ArticleCategories { get; }

    DbSet<ArticleTag> ArticleTags { get; }

    DbSet<ArticleTagRelation> ArticleTagRelations { get; }

    DbSet<ArticleVisitStatistic> ArticleVisitStatistics { get; }
}