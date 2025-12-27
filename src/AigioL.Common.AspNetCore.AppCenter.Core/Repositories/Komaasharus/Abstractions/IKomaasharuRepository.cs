using AigioL.Common.AspNetCore.AppCenter.Entities.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus;
using AigioL.Common.AspNetCore.AppCenter.Models.Komaasharus.Summaries;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Repositories.Komaasharus.Abstractions;

public partial interface IKomaasharuRepository : IRepository<Komaasharu, Guid>, IEFRepository
{
    /// <summary>
    /// 添加点击或曝光计数器
    /// </summary>
    /// <param name="id">广告 Id</param>
    /// <param name="count">查询展示次数</param>
    /// <param name="clickCount">点击次数</param>
    /// <returns></returns>
    Task AddCounterAsync(Guid id, long count, long clickCount);

    /// <summary>
    /// 获取在有效期内的所有广告实体数据
    /// </summary>
    /// <returns></returns>
    Task<Komaasharu[]> GetAllEntitiesAsync();

    /// <summary>
    /// 获取当前可用的（广告在有效期内）所有广告的缓存数据
    /// </summary>
    /// <returns></returns>
    Task<KomaasharuRedisModel[]> GetCacheModelsAsync();

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
    /// <summary>
    /// 表格查询
    /// </summary>
    Task<PagedModel<KomaasharuTableItem>> QueryAsync(
        string? name,
        KomaasharuType? type,
        KomaasharuOrientation? orientation,
        DateTimeOffset?[]? startTime,
        DateTimeOffset?[]? endTime,
        bool? expired,
        bool? disable,
        string? orderBy,
        bool? desc,
        int current,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据编辑模型添加或更新一行数据
    /// </summary>
    Task<int> InsertOrUpdateAsync(KomaasharuEdit model);

    /// <summary>
    /// 根据主键获取编辑模型
    /// </summary>
    Task<KomaasharuEdit?> GetEditByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置禁用状态
    /// </summary>
    Task<int> SetDisableAsync(Guid id, bool disable);

    /// <summary>
    /// 展示记录按天统计
    /// </summary>
    Task<StatisticsKomaasharuResponse[]?> GetStatistics(
        Guid id,
        CancellationToken cancellationToken = default);
}