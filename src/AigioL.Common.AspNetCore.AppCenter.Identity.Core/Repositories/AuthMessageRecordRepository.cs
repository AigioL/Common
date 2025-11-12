using AigioL.Common.AspNetCore.AppCenter.Constants;
using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using AigioL.Common.SmsSender.Services;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;

sealed partial class AuthMessageRecordRepository<TDbContext> :
    Repository<TDbContext, AuthMessageRecord, Guid>,
    IAuthMessageRecordRepository
    where TDbContext : DbContext, IIdentityDbContext
{
    public AuthMessageRecordRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    static AuthMessageType GetDefaultAuthMessageType() => AuthMessageType.PhoneNumber;

    static bool IsPhoneNumberOrEmail(AuthMessageType authMessageType) => authMessageType switch
    {
        AuthMessageType.Email => false,
        AuthMessageType.PhoneNumber => true,
        _ => throw new ArgumentOutOfRangeException(nameof(authMessageType), authMessageType, null),
    };

    public async Task<AuthMessageRecord?> CheckAuthMessageAsync(
        ISmsSender smsSender,
        string phoneNumberOrEmail,
        string? phoneNumberRegionCode,
        string message,
        SmsCodeType useType,
        AuthMessageType? type = null)
    {
        var type_ = type ?? GetDefaultAuthMessageType();

        var lastEffectiveRecord = await GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(type_, phoneNumberOrEmail, phoneNumberRegionCode, useType);
        if (lastEffectiveRecord == null || lastEffectiveRecord.Content != message)
        {
            return null;
        }
        // ↑ 上一条有效纪录不存在或者验证码值不相同，返回null

        if (!lastEffectiveRecord.EverCheck) // 如果这条纪录没有校验过
        {
            lastEffectiveRecord.EverCheck = true; // 设置此条纪录已经校验过
        }

        if (lastEffectiveRecord.CheckFailuresCount >= SMSConstants.MaxCheckFailuresCount)
        {
            lastEffectiveRecord.Abandoned = true;
        }
        else
        {
            if (IsPhoneNumberOrEmail(type_) && smsSender != null && smsSender.SupportCheck)
            {
                /* var check_provide_result =*/
                await smsSender.CheckSmsAsync(phoneNumberOrEmail, message);
            }

            lastEffectiveRecord.CheckSuccess = true;
        }

        if (!lastEffectiveRecord.CheckSuccess)
        {
            lastEffectiveRecord.CheckFailuresCount++;
            if (lastEffectiveRecord.CheckFailuresCount >= SMSConstants.MaxCheckFailuresCount)
            {
                lastEffectiveRecord.Abandoned = true; // 验证错误次数太多，此条纪录标记废弃
            }
        }

        await db.SaveChangesAsync();
        return lastEffectiveRecord;
    }

    public async Task<DateTimeOffset?> GetLastSendSmsTime(string phoneNumberOrEmail, string? phoneNumberRegionCode, SmsCodeType? requestType = null, AuthMessageType? type = null)
    {
        IQueryable<AuthMessageRecord> query = Entity.AsNoTrackingWithIdentityResolution();

        if (requestType.HasValue)
        {
            var requestTypeValue = requestType.Value;
            query = query.Where(x => x.RequestType.Equals(requestTypeValue));
        }

        var type_ = type ?? GetDefaultAuthMessageType();
        if (type_ == AuthMessageType.PhoneNumber)
        {
            query = query.Where(x => x.PhoneNumberRegionCode == phoneNumberRegionCode);
        }

        var query2 = from x in query
                     where x.PhoneNumber == phoneNumberOrEmail && x.Type == type_
                     orderby x.CreationTime descending
                     select (DateTimeOffset?)x.CreationTime;

        var r = await query2.Take(1).FirstOrDefaultAsync();
        return r;
    }

    public async Task<AuthMessageRecord?> GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(AuthMessageType type, string phoneNumberOrEmail, string? phoneNumberRegionCode, SmsCodeType useType)
    {
        // 过滤条件 30分钟之前创建的短信 没有废弃的(!Abandoned) 并且是 没有成功的(!CheckSuccess)
        var before = DateTimeOffset.Now.AddSeconds(-SMSConstants.SmsSendPeriodValidity); // 30分钟之前
        IQueryable<AuthMessageRecord> query = from x in Entity
                                              where x.RequestType.Equals(useType) &&
                                              x.CreationTime >= before &&
                                              !x.Abandoned &&
                                              !x.CheckSuccess &&
                                              x.Type == type
                                              orderby x.CreationTime descending // 时间倒排序
                                              select x;

        var isPhoneNumberOrEmail = IsPhoneNumberOrEmail(type);
        if (isPhoneNumberOrEmail)
        {
            query = query.Where(x => x.PhoneNumber == phoneNumberOrEmail && x.PhoneNumberRegionCode == phoneNumberRegionCode);
        }
        else
        {
            query = query.Where(x => x.Email == phoneNumberOrEmail);
        }

        var r = await query.Take(1).FirstOrDefaultAsync();
        return r;
    }

    public async Task<bool> IsMaxSendSmsDay(string phoneNumber, string? phoneNumberRegionCode, byte? maxSendSmsDay = null, AuthMessageType? type = null)
    {
        var type_ = type ?? GetDefaultAuthMessageType();
        var maxSendSmsDay_ = maxSendSmsDay ?? SMSConstants.MaxSendSmsDay;
        var today = DateTimeOffset.Now.Date;
        var tomorrow = today.AddDays(1);

        var count = await Entity
            .CountAsync(x => x.PhoneNumber == phoneNumber && x.PhoneNumberRegionCode == phoneNumberRegionCode &&
                x.CreationTime >= today &&
                x.CreationTime < tomorrow &&
                x.Type == type_);

        return count > maxSendSmsDay_;
    }

    //public Task<PagedModel<dynamic>> QueryAsync(Guid? userId, string? phoneNumber, string? phoneNumberRegionCode, string? nickName, DateTimeOffset?[]? creationTime, string? email, SmsCodeType? requestType, bool? everCheck, bool? checkSuccess, string? orderBy, bool? desc, int current = 1, int pageSize = 10)
    //{
    //    var query = Entity.AsNoTrackingWithIdentityResolution();

    //    if (userId.HasValue)
    //        query = query.Where(x => x.UserId == userId);
    //    if (!string.IsNullOrWhiteSpace(phoneNumber))
    //        query = query.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(phoneNumber));
    //    if (!string.IsNullOrWhiteSpace(nickName))
    //        query = query.Where(a => a.User!.NickName!.ToLower().Contains(nickName.ToLower()));
    //    if (creationTime != null && creationTime.Length == 2)
    //    {
    //        if (creationTime[0].HasValue)
    //            query = query.Where(x => x.CreationTime >= creationTime[0]);
    //        if (creationTime[1].HasValue)
    //            query = query.Where(x => x.CreationTime < creationTime[1]);
    //    }
    //    if (!string.IsNullOrWhiteSpace(email))
    //        query = query.Where(x => x.Email == email);

    //    if (requestType.HasValue)
    //        query = query.Where(x => x.RequestType == requestType);

    //    if (everCheck.HasValue)
    //        query = query.Where(x => x.EverCheck == everCheck);
    //    if (checkSuccess.HasValue)
    //        query = query.Where(x => x.CheckSuccess == checkSuccess);
    //    if (!string.IsNullOrEmpty(orderBy))
    //        query = desc != true ? query.OrderByDynamic($"x=>x.{orderBy}") : query.OrderByDescendingDynamic($"x=>x.{orderBy}");
    //    else
    //        query = query.OrderByDescending(x => x.CreationTime);
    //    var r = await query
    //        //.OrderByDescending(x => x.CreationTime)
    //        .Include(x => x.User)
    //        .ProjectTo<AuthMessageRecordTableItem>(mapper.ConfigurationProvider)
    //        .PagingAsync(current, pageSize, RequestAborted);
    //    return r;
    //}
}