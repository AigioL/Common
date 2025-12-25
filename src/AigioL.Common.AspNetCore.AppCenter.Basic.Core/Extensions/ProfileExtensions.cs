using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace AutoMapper;

/// <summary>
/// AutoMapper Configuration
/// <para>https://docs.automapper.io/en/stable/Configuration.html</para>
/// </summary>
public static partial class ProfileExtensions
{
    public static void AddBasicProfile(this Profile p)
    {
        p.CreateMap<ArticleCategory, ArticleCategoryTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser!.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser!.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser!.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser!.NickName))
            .ForMember(d => d.ArticleCount, opt => opt.MapFrom(s => s.Articles!.Count));
        p.CreateMap<ArticleCategory, AddOrEditArticleCategoryModel>();
        p.CreateMap<ArticleCategory, ArticleCategoryTreeNodeModel>();

        p.CreateMap<Article, ArticleTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser!.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser!.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser!.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser!.NickName))
            .ForMember(d => d.TagIds, opt => opt.MapFrom(s => s.Tags.Select(static x => x.Id).ToArray()));
        p.CreateMap<Article, AddOrEditArticleCategoryModel>();
        p.CreateMap<AddOrEditArticleCategoryModel, Article>();

        p.CreateMap<ArticleTag, ArticleTagTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser!.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser!.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser!.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser!.NickName))
            .ForMember(d => d.ArticleCount, opt => opt.MapFrom(s => s.Articles!.Count));
        p.CreateMap<ArticleTag, ArticleTagOptionItemModel>();
    }
}
