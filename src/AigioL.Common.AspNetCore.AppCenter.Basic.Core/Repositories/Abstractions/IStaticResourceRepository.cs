using AigioL.Common.AspNetCore.AppCenter.Basic.Entities.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.FileSystem;
using AigioL.Common.AspNetCore.AppCenter.Basic.Models.Storage;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public partial interface IStaticResourceRepository : IRepository<StaticResource, Guid>, IEFRepository
{
    Task<(string? filePath, CloudFileType fileType)> GetFilePathBySha384WithFileExtAsync(
        string sha384,
        string fileExt,
        CancellationToken cancellationToken = default);
}

partial interface IStaticResourceRepository // 管理后台
{
    /// <summary>
    /// 表格查询
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="fileType">文件类型</param>
    /// <param name="orderBy">排序字段</param>
    /// <param name="sha384">哈希值</param>
    /// <param name="desc">排序: false 为降序，true 为升序 </param>
    /// <param name="current"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PagedModel<StaticResourceTableItemModel>> QueryAsync(
        string? fileName,
        string? filePath,
        CloudFileType? fileType,
        string? sha384,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Guid? operatorUserId,
        AddOrEditStaticResourceModel model,
        CancellationToken cancellationToken = default);

    Task<bool> InsertAsync(
        Guid? createUserId,
        AddOrEditStaticResourceModel model,
        CancellationToken cancellationToken = default);
}

partial interface IStaticResourceRepository // 微服务
{

}