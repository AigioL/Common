using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AigioL.Common.SmsSender.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class AuthMessageRecordRepository<TDbContext> :
    Repository<TDbContext, UserDelete, Guid>,
    IAuthMessageRecordRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public AuthMessageRecordRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public Task<AuthMessageRecord?> CheckAuthMessageAsync(ISmsSender smsSender, string phoneNumberOrEmail, string? phoneNumberRegionCode, string message, SmsCodeType useType, AuthMessageType? type = null)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(AuthMessageRecord entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AuthMessageRecord?> FirstOrDefaultAsync(Expression<Func<AuthMessageRecord, bool>> predicate, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public Task<DateTimeOffset?> GetLastSendSmsTime(string phoneNumberOrEmail, string? phoneNumberRegionCode, SmsCodeType? requestType = null, AuthMessageType? type = null)
    {
        throw new NotImplementedException();
    }

    public Task<AuthMessageRecord?> GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(AuthMessageType type, string phoneNumberOrEmail, string? phoneNumberRegionCode, SmsCodeType useType)
    {
        throw new NotImplementedException();
    }

    public object GetPrimaryKey(AuthMessageRecord entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> InsertAsync(AuthMessageRecord entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsMaxSendSmsDay(string phoneNumber, string? phoneNumberRegionCode, byte? maxSendSmsDay = null, AuthMessageType? type = null)
    {
        throw new NotImplementedException();
    }

    public Task<PagedModel<dynamic>> QueryAsync(Guid? userId, string? phoneNumber, string? phoneNumberRegionCode, string? nickName, DateTimeOffset?[]? creationTime, string? email, SmsCodeType? requestType, bool? everCheck, bool? checkSuccess, string? orderBy, bool? desc, int current = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync(AuthMessageRecord entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    ValueTask<AuthMessageRecord?> IRepository<AuthMessageRecord, Guid>.FindAsync(Guid primaryKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}