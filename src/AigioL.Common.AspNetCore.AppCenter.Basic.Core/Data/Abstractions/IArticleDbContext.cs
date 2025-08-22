using AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Data.Abstractions
{
    public interface IArticleDbContext : IDbContextBase
    {
        DbSet<Article> Articles { get; }

        DbSet<ArticleCategory> ArticleCategories { get; }

        DbSet<ArticleTag> ArticleTags { get; }

        DbSet<ArticleTagRelation> ArticleTagRelations { get; }

        DbSet<ArticleVisitStatistic> ArticleVisitStatistics { get; }
    }
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IArticleDbContext
    {
        public DbSet<Article> Articles { get; set; } = null!;

        public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;

        public DbSet<ArticleTag> ArticleTags { get; set; } = null!;

        public DbSet<ArticleTagRelation> ArticleTagRelations { get; set; } = null!;

        public DbSet<ArticleVisitStatistic> ArticleVisitStatistics { get; set; } = null!;
    }
}
#endif