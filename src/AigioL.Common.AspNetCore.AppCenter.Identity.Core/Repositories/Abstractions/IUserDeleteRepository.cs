using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public interface IUserDeleteRepository : IRepository<UserDelete, Guid>, IEFRepository
{
    /// <summary>
    /// 删除账号（用户注销）
    /// </summary>
    Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default);
}
