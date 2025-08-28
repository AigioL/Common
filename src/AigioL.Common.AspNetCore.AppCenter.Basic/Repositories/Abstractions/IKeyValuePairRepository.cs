using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using KeyValuePair = AigioL.Common.AspNetCore.AppCenter.Basic.Entities.KeyValuePairs.KeyValuePair;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Repositories.Abstractions;

public interface IKeyValuePairRepository : IRepository<KeyValuePair, string>, IEFRepository
{
    /// <summary>
    /// 查询指定键值对的值
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<KeyValuePair?> QueryAsync(string id);
}
