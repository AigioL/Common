using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using System.Globalization;
using KeyValuePair = AigioL.Common.AspNetCore.AppCenter.Entities.KeyValuePair;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

public partial interface IKeyValuePairRepository : IRepository<KeyValuePair, string>, IEFRepository
{
    /// <summary>
    /// 查询指定键值对的值
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<KeyValuePair?> QueryAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取视图布局模型，优先从缓存中获取
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(ViewLayoutModel? m, string langKey)> GetViewLayoutModelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="SetViewLayoutModelAsync"/> 的默认缓存过期时间，单位：分钟
    /// </summary>
    protected const int KeyExpirationFromMinutes_ViewLayout = 10;

    /// <summary>
    /// 设置视图布局模型，并更新缓存
    /// </summary>
    /// <param name="m"></param>
    /// <param name="langKey"></param>
    /// <param name="expirationFromMinutes"></param>
    /// <returns></returns>
    Task SetViewLayoutModelAsync(ViewLayoutModel m, string? langKey = null, int? expirationFromMinutes = null);
}
