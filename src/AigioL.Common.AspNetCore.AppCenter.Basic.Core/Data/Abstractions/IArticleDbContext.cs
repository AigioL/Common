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

#if !REMOVE_APP_DBCONTEXT
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AigioL.Common.AspNetCore.Data
{
    partial class AppDbContext : IArticleDbContext
    {
        public DbSet<Article> Articles { get; set; } = null!;

        public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;

        public DbSet<ArticleTag> ArticleTags { get; set; } = null!;

        public DbSet<ArticleTagRelation> ArticleTagRelations { get; set; } = null!;

        public DbSet<ArticleVisitStatistic> ArticleVisitStatistics { get; set; } = null!;
    }
}
#endif