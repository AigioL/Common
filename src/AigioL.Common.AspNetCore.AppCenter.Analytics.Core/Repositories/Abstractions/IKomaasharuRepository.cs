using AigioL.Common.AspNetCore.AppCenter.Analytics.Entities.Komaasharu;
using AigioL.Common.AspNetCore.AppCenter.Analytics.Models.Komaasharu;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Analytics.Repositories.Abstractions;

public partial interface IKomaasharuRepository : IRepository<Komaasharu, Guid>, IEFRepository
{
    ///// <summary>
    ///// 添加点击或曝光计数器
    ///// </summary>
    ///// <param name="id">广告 Id</param>
    ///// <param name="count">查询展示次数</param>
    ///// <param name="clickCount">点击次数</param>
    ///// <returns></returns>
    //Task AddCounterAsync(Guid id, long count, long clickCount);

    /// <summary>
    /// 获取在有效期内的所有广告实体数据
    /// </summary>
    /// <returns></returns>
    Task<Komaasharu[]> GetAllEntitiesAsync();

    /// <summary>
    /// 获取全部数据（用于客户端）
    /// </summary>
    Task<KomaasharuModel[]> GetAllAsync(
        KomaasharuType? type = null,
        DevicePlatform2? platform = null,
        DeviceIdiom? deviceIdiom = null,
        CancellationToken cancellationToken = default);
}

partial interface IKomaasharuRepository // 管理后台
{
}