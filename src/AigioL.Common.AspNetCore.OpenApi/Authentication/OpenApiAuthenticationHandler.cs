using AigioL.Common.AspNetCore.PartnerCenter.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace AigioL.Common.AspNetCore.OpenApi.Authentication;

public sealed class OpenApiAuthenticationHandler<TDbContext>(
    IOptionsMonitor<OpenApiAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    TDbContext db) :
    OpenApiAuthenticationHandlerBase(options, logger, encoder)
    where TDbContext : DbContext
{
    protected override async ValueTask<(ReadOnlyMemory<byte> appSecret, string appName)> GetAppSecretAsync(
        ReadOnlyMemory<char> appAccessKey,
        CancellationToken cancellationToken = default)
    {
        if (ShortGuid.TryParse(appAccessKey.Span, out Guid pcUserId))
        {
            var query = from m in db.Set<PCUser>().AsNoTrackingWithIdentityResolution()
                        where m.Id == pcUserId
                        select new { m.AppSecret, m.NickName, };
            var r = await query.SingleOrDefaultAsync(cancellationToken);
            if (r != null && r.AppSecret != null && r.AppSecret.Length > 0)
            {
                return (r.AppSecret, r.NickName);
            }
        }
        return default;
    }
}
