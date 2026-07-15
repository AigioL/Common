using AigioL.Common.AspNetCore.OpenApi.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace AigioL.Common.AspNetCore.OpenApi.Repositories;

sealed partial class OpenIddictAuthorizationRepository<TDbContext>(TDbContext dbContext) :
    //Repository<TDbContext, OpenIddictEntityFrameworkCoreAuthorization, string>,
    IOpenIddictAuthorizationRepository
    where TDbContext : DbContext
{
    readonly TDbContext db = dbContext;

    /// <inheritdoc/>
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Entity { get; } = dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>();

    public async Task<Guid?> GetUserIdByOpenIdAsync(Guid openId, CancellationToken cancellationToken = default)
    {
        var id = openId.ToString();

        var query = from m in Entity.AsNoTrackingWithIdentityResolution()
                    where m.Status == "valid" && m.Type == "permanent"
                    && m.Id == id
                    select m.Subject;

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        if (ShortGuid.TryParse(r, out Guid userId))
        {
            return userId;
        }
        return null;
    }
}
