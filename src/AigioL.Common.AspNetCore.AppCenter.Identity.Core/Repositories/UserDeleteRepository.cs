using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class UserDeleteRepository<TDbContext> :
    Repository<TDbContext, UserDelete, Guid>,
    IUserDeleteRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserDeleteRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task DeleteAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await db.GetDatabase().CreateExecutionStrategy().ExecuteAsync(DeleteAccountCoreAsync);

        async Task DeleteAccountCoreAsync()
        {
            using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            #region 用户注销业务

            var user = await db.Users.FindAsync([userId], cancellationToken);
            if (user == null)
            {
                return;
            }

            await db.Entry(user).Collection(x => x.ExternalAccounts!).LoadAsync(cancellationToken);
            // 添加用户注销记录
            var userDelete = new UserDelete()
            {
                UserId = user.Id,
                // 保留用户的一些信息
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                NickName = user.NickName,
                PersonalizedSignature = user.PersonalizedSignature,
                AvatarUrl = user.AvatarUrl,
                Gender = user.Gender,
                BirthDate = user.BirthDate,
                //BirthDateTimeZone = user.BirthDateTimeZone,
                AreaId = user.AreaId,
                // 记录“注销用户”的“外部账号”的绑定关系（曾经）
                ExternalAccounts = user.ExternalAccounts,
            };
            await db.UserDeletes.AddAsync(userDelete, cancellationToken);

            // 清除用户的隐私信息
            user.PhoneNumber = null;
            user.Email = null;
            user.NormalizedEmail = null;
            user.EmailConfirmed = false;
            user.NickName = null;
            user.PersonalizedSignature = null;
            user.AvatarUrl = null;
            user.Gender = Gender.Unknown;
            user.BirthDate = null;
            //user.BirthDateTimeZone = 0;
            user.AreaId = null;

            // 解绑外部账号
            user.ExternalAccounts!.ForEach(a => a.UserId = null);

            // 清除用户设备-会级联删除用户的 JWT token
            await db.UserDevices.Where(ud => ud.UserId == userId).ExecuteDeleteAsync(cancellationToken);

            // 设置锁定状态，以阻止用户登录
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await db.SaveChangesAsync(cancellationToken);

            #endregion 用户注销业务

            await transaction.CommitAsync(cancellationToken);
        }
    }
}