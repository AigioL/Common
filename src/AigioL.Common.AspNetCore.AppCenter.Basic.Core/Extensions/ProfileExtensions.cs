using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.OfficialMessages;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.AppVersions;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Articles;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Notice;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;
using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using KeyValuePair = AigioL.Common.AspNetCore.AppCenter.Entities.KeyValuePair;

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
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.ArticleCount, opt => opt.MapFrom(s => s.Articles == null ? default : s.Articles.Count));
        p.CreateMap<ArticleCategory, AddOrEditArticleCategoryModel>()
            .ReverseMap();
        p.CreateMap<ArticleCategory, ArticleCategoryTreeNodeModel>();

        p.CreateMap<Article, ArticleTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.TagIds, opt => opt.MapFrom(s => s.Tags.Select(static x => x.Id).ToArray()));
        p.CreateMap<Article, AddOrEditArticleCategoryModel>()
            .ReverseMap();

        p.CreateMap<ArticleTag, ArticleTagTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.ArticleCount, opt => opt.MapFrom(s => s.Articles == null ? default : s.Articles.Count));
        p.CreateMap<ArticleTag, ArticleTagOptionItemModel>();

        p.CreateMap<KeyValuePair, KeyValuePairTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName));

        p.CreateMap<OfficialMessage, OfficialMessageTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
             .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName))
            .ForMember(d => d.UserViewable, opt => opt.MapFrom(s => s.PushTime <= DateTime.UtcNow && (!s.ExpireTime.HasValue || DateTime.UtcNow < s.ExpireTime)))
            .ForMember(d => d.PushAppVersions, opt => opt.MapFrom(s => s.TargetAppVers!.Select(a => a.Id)))
            .ReverseMap();
        p.CreateMap<OfficialMessage, AddOrEditOfficialMessageModel>()
            .ReverseMap();

        p.CreateMap<StaticResource, StaticResourceTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
             .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName));
        p.CreateMap<StaticResource, AddOrEditStaticResourceModel>()
            .ReverseMap();
        p.CreateMap<StaticResourceUploadRecord, StaticResourceUploadRecordItemModel>()
            .ForMember(d => d.User, opt => opt.MapFrom(s => s.User == null ? default : s.User.NickName))
            .ForMember(d => d.BMUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
            .ForMember(d => d.BMUserId, opt => opt.MapFrom(s => s.CreateUserId));

        p.CreateMap<AppVer, AppVersionTableItemModel>()
            .ForMember(d => d.CreateUserId, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.Id))
            .ForMember(d => d.CreateUser, opt => opt.MapFrom(s => s.CreateUser == null ? default : s.CreateUser.NickName))
             .ForMember(d => d.OperatorUserId, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.Id))
            .ForMember(d => d.OperatorUser, opt => opt.MapFrom(s => s.OperatorUser == null ? default : s.OperatorUser.NickName));
        p.CreateMap<AppVer, AddOrEditAppVersionModel>()
            .ReverseMap();
    }
}
