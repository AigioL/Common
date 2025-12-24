using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class UserDeviceRepository<TDbContext> :
    Repository<TDbContext, UserDevice, Guid>,
    IUserDeviceRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserDeviceRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class UserDeviceRepository<TDbContext>
{
    public async Task<PagedModel<UserDeviceTableItem>> QueryAsync(
        Guid? userId,
        string? nickName,
        string? deviceName,
        string? deviceId,
        DateTimeOffset?[]? lastLoginTime,
        bool? isTrust,
        DevicePlatform2? platform,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.UserDevices.AsNoTrackingWithIdentityResolution();
        var queryM = query
            //.Include(x => x.User)
            .ProjectTo<UserDeviceTableItem>(mapper.ConfigurationProvider);

        if (userId.HasValue)
            queryM = queryM.Where(u => u.UserId == userId);
        if (!string.IsNullOrEmpty(deviceName))
            queryM = queryM.Where(a => a.DeviceName!.Contains(deviceName));
        if (!string.IsNullOrEmpty(deviceId))
            queryM = queryM.Where(a => a.DeviceId!.Contains(deviceId));

        if (lastLoginTime != null && lastLoginTime.Length == 2)
        {
            if (lastLoginTime[0].HasValue)
                queryM = queryM.Where(x => x.LastLoginTime >= lastLoginTime[0]);
            if (lastLoginTime[1].HasValue)
                queryM = queryM.Where(x => x.LastLoginTime < lastLoginTime[1]);
        }
        if (!string.IsNullOrEmpty(nickName))
            query = query.Where(a => a.User!.NickName!.Contains(nickName));
        if (isTrust.HasValue)
            queryM = queryM.Where(a => a.IsTrust == isTrust);
        if (platform.HasValue)
            queryM = queryM.Where(a => a.Platform == platform);
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = query.OrderByPropertyName(orderBy, desc);
        }
        else
        {
            query = query.OrderByDescending(x => x.Id);
        }
        var r = await queryM.PagingAsync(current, pageSize, cancellationToken);
        return r;
    }

    public async Task<bool> SignOut(Guid deviceId)
    {
        await db.UserJsonWebTokens
            .Where(a => a.UserDeviceId == deviceId)
            .ExecuteDeleteAsync();
        await db.UserRefreshJsonWebTokens
            .Include(x => x.JWT)
            .Where(a => a.JWT!.UserDeviceId == deviceId)
            .ExecuteDeleteAsync();
        return true;
    }
}