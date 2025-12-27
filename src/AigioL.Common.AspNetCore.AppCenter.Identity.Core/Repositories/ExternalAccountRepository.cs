using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

public sealed partial class ExternalAccountRepository<TDbContext> :
    Repository<TDbContext, ExternalAccount, Guid>,
    IExternalAccountRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public ExternalAccountRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class ExternalAccountRepository<TDbContext> // 管理后台
{
    public async Task<PagedModel<ExternalAccountTableItem>> QueryAsync(
        Guid? userId,
        string? externalAccountId,
        ExternalLoginChannel? type,
        string? nickName,
        string? givenName,
        string? surname,
        Gender? gender,
        string? email,
        string? userNickName,
        DateTimeOffset?[]? createTime,
        DateTimeOffset?[]? updateTime,
        string? orderBy,
        bool? desc,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.ExternalAccounts.AsNoTrackingWithIdentityResolution();

        if (userId.HasValue)
            query = query.Where(u => u.UserId == userId);
        if (!string.IsNullOrEmpty(externalAccountId))
            query = query.Where(a => a.ExternalAccountId.Contains(externalAccountId));
        if (type.HasValue)
            query = query.Where(a => a.Type == type);
        if (!string.IsNullOrEmpty(nickName))
            query = query.Where(a => a.NickName!.Contains(nickName));
        if (!string.IsNullOrEmpty(givenName))
            query = query.Where(a => a.GivenName!.Contains(givenName));
        if (!string.IsNullOrEmpty(surname))
            query = query.Where(a => a.Surname!.Contains(surname));
        if (gender.HasValue)
            query = query.Where(a => a.Gender == gender);
        if (!string.IsNullOrEmpty(email))
            query = query.Where(a => a.Email!.Contains(email));
        if (!string.IsNullOrEmpty(userNickName))
            query = query.Where(a => a.User!.NickName!.Contains(userNickName));

        if (createTime != null && createTime.Length == 2)
        {
            if (createTime[0].HasValue)
                query = query.Where(x => x.CreateTime >= createTime[0]);
            if (createTime[1].HasValue)
                query = query.Where(x => x.CreateTime < createTime[1]);
        }
        if (updateTime != null && updateTime.Length == 2)
        {
            if (updateTime[0].HasValue)
                query = query.Where(x => x.UpdateTime >= updateTime[0]);
            if (updateTime[1].HasValue)
                query = query.Where(x => x.UpdateTime < updateTime[1]);
        }
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }

        var r = await query
            .ProjectTo<ExternalAccountTableItem>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);
        return r;
    }
}