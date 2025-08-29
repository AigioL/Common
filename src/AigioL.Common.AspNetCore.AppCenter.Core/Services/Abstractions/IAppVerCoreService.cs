using AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Services.Abstractions;

public interface IAppVerCoreService
{
    Task<IReadOnlyAppVer?> GetAsync(HttpContext context, bool fromHeaderOrQuery);

    ValueTask<IReadOnlyAppVer?> FindAsync(Guid id, CancellationToken cancellationToken = default);
}
