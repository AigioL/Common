using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class UserMembershipRepository<TDbContext> :
    Repository<TDbContext, UserDelete, Guid>,
    IUserMembershipRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public UserMembershipRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public Task<bool> AddUserMembershipFlagAsync(Guid userId, MembershipLicenseFlags membershipLicenseFlags)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(UserMembership entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UserMembership?> FirstOrDefaultAsync(Expression<Func<UserMembership, bool>> predicate, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public object GetPrimaryKey(UserMembership entity)
    {
        throw new NotImplementedException();
    }

    public Task<MembershipInfo?> GetUserMembershipAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertAsync(UserMembership entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveUserMembershipFlagAndCheckExpiredAsync(Guid userId, MembershipLicenseFlags membershipLicenseFlags)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync(UserMembership entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    ValueTask<UserMembership?> IRepository<UserMembership, Guid>.FindAsync(Guid primaryKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}