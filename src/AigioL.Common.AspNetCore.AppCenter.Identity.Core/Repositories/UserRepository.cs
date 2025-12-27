using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.EntityFrameworkCore.Extensions;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class UserRepository<TDbContext> :
    Repository<TDbContext, User, Guid>,
    IUserRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }
}

partial class UserRepository<TDbContext>
{
    public async Task<PagedModel<UserTableItem>> QueryAsync(
        Guid? id,
        string? openId,
        UserType? userType,
        string? nickName,
        Gender? gender,
        DateTimeOffset?[]? lastLoginTime,
        bool? isLockout,
        string? phoneNumber,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        bool hidePhoneNumberMiddleFour = true,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.Users.AsNoTrackingWithIdentityResolution();

        if (id.HasValue)
            query = query.Where(u => u.Id == id);
        //if (openId != null && await GetUserIdFromOpenId(openId) is Guid userId)
        //    query = query.Where(u => u.Id == userId);
        if (userType.HasValue)
        {
            // 用户类型字段中值 0 的用户（可能是历史数据），类型一律显示为普通用户
            const UserType LegacyUserType = 0;

            if (userType == LegacyUserType)
                userType = UserType.Ordinary;
            if (userType == UserType.Ordinary)
                query = query.Where(u => new[] { UserType.Ordinary, LegacyUserType }.Contains(u.UserType));
            else
                query = query.Where(u => u.UserType == userType);
        }
        if (!string.IsNullOrEmpty(nickName))
            query = query.Where(a => a.NickName!.Contains(nickName));
        if (gender.HasValue)
            query = query.Where(u => u.Gender == gender);
        if (isLockout.HasValue)
            query = isLockout.Value
                ? query.Where(u => u.LockoutEnd > DateTime.Now)
                : query.Where(u => u.LockoutEnd == null || u.LockoutEnd < DateTime.Now);
        if (lastLoginTime != null && lastLoginTime.Length == 2)
        {
            if (lastLoginTime[0].HasValue)
                query = query.Where(x => x.LastLoginTime >= lastLoginTime[0]);
            if (lastLoginTime[1].HasValue)
                query = query.Where(x => x.LastLoginTime < lastLoginTime[1]);
        }
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            if (IsPhoneNumberValidFormat(phoneNumber))
                query = query.Where(u => u.PhoneNumber == phoneNumber);
            else
                query = query.Where(u => u.PhoneNumber!.Contains(phoneNumber));
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
            .ProjectTo<UserTableItem>(mapper.ConfigurationProvider)
            .PagingAsync(current, pageSize, cancellationToken);

        if (r.DataSource != null)
        {
            if (hidePhoneNumberMiddleFour)
            {
                foreach (var it in r.DataSource)
                {
                    it.PhoneNumber = IPhoneNumber.ToStringHideMiddleFour(it.PhoneNumber);
                }
            }
        }

        return r;
    }

    public async Task<int> UpdateAsync(UserEdit model)
    {
        if (model.Id == default)
            return default;

        var r = await db.Users.Where(u => u.Id == model.Id)
            .ExecuteUpdateAsync(a =>
                a.SetProperty(u => u.UserType, u => model.UserType)
                 .SetProperty(u => u.NickName, u => model.NickName)
                 .SetProperty(u => u.PersonalizedSignature, u => model.PersonalizedSignature));
        return r;
    }

    public async Task<int> UpdateElevatedAsync(UserEdit model)
    {
        if (model.Id == default)
            return default;

        var r = await db.Users.Where(u => u.Id == model.Id)
            .ExecuteUpdateAsync(a =>
                a.SetProperty(u => u.UserType, u => model.UserType)
                 .SetProperty(u => u.NickName, u => model.NickName)
                 .SetProperty(u => u.PersonalizedSignature, u => model.PersonalizedSignature)

                 // 高级权限字段
                 .SetProperty(u => u.PhoneNumber, u => model.PhoneNumber)
                 .SetProperty(u => u.PhoneNumberRegionCode, u => model.PhoneNumberRegionCode));
        return r;
    }

    public async Task<UserEdit?> GetEditByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var query = db.Users.AsNoTrackingWithIdentityResolution();
        var r = await query.Where(x => x.Id == id)
            .ProjectTo<UserEdit>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<UserSearchModel> SearchUsers(
        string text,
        ushort takeCount = 20,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = db.Users.AsNoTrackingWithIdentityResolution();
        if (TryGetGuid(text, out var userIdG))
        {
            query = query.Where(u => u.Id == userIdG);
        }
        else if (IsPhoneNumberValidFormat(text))
        {
            query = query.Where(u => u.PhoneNumber == text);
        }
        else
        {
            query = query.Where(u => u.NickName!.Contains(text));
        }

        UserSearchModel r = new()
        {
            Count = await query.CountAsync(),
            Items = await query.OrderBy(u => u.CreateTime)
                .Select(u => new UserSearchItemModel
                {
                    Id = u.Id,
                    NickName = u.NickName,
                    PhoneNumber = u.PhoneNumber,
                    AvatarUrl = u.AvatarUrl,
                })
                .Take(takeCount)
                .ToListAsync(),
        };
        return r;
    }

    public async Task<UserWalletModel?> GetWalletByUserIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var mapper = serviceProvider.GetRequiredService<IMapper>();
        var r = await db.UserWallets.Where(a => a.Id == id)
            .ProjectTo<UserWalletModel>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<bool> SetUserLockoutStateAsync(Guid id, bool lockout)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            return false;
        }

        if (lockout)
        {
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }
        else
        {
            user.LockoutEnabled = true;
            user.LockoutEnd = null;
        }

        var r = await db.SaveChangesAsync(CancellationToken.None);
        return r > 0;
    }

    static bool IsPhoneNumberValidFormat(string phoneNumber)
    {
        return phoneNumber.Length == 11 && phoneNumber[0] == '1';
    }

    static bool TryGetGuid(string s, out Guid guid)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            if (Guid.TryParse(s, out guid))
            {
                return true;
            }
            else if (s.Length <= 38)
            {
                Span<char> chars = stackalloc char[s.Length];
                s.CopyTo(chars);
                chars.Replace('+', '-');
                chars.Replace('/', '_');
                chars = chars.TrimEnd();
                if (ShortGuid.TryParse(chars, out guid))
                {
                    return true;
                }
            }
        }
        guid = default;
        return false;
    }
}
