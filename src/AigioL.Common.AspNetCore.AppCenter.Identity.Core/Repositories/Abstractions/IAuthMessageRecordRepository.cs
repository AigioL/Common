using AigioL.Common.AspNetCore.AppCenter.Entities;
using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.SmsSender.Services;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;

public partial interface IAuthMessageRecordRepository : IRepository<AuthMessageRecord, Guid>
{
}

partial interface IAuthMessageRecordRepository // 微服务
{
    /// <summary>
    /// 获取最近的没有校验和没有废弃的一条验证码
    /// </summary>
    /// <param name="type">验证码类型</param>
    /// <param name="phoneNumberOrEmail">手机号码或邮箱地址</param>
    /// <param name="phoneNumberRegionCode"></param>
    /// <param name="useType"></param>
    /// <returns></returns>
    Task<AuthMessageRecord?> GetMostRecentVerificationCodeWithoutChecksumAndMoDiscard(
        AuthMessageType type,
        string phoneNumberOrEmail,
        string? phoneNumberRegionCode,
        SmsCodeType useType);

    /// <summary>
    /// 校验验证码
    /// </summary>
    /// <param name="smsSender"></param>
    /// <param name="phoneNumberOrEmail"></param>
    /// <param name="phoneNumberRegionCode"></param>
    /// <param name="message"></param>
    /// <param name="useType"></param>
    /// <param name="type">验证码类型</param>
    /// <returns></returns>
    Task<AuthMessageRecord?> CheckAuthMessageAsync(
        ISmsSender smsSender,
        string phoneNumberOrEmail,
        string? phoneNumberRegionCode,
        string message,
        SmsCodeType useType,
        AuthMessageType? type = null);

    /// <summary>
    /// 获取手机号上次发送验证码的时间
    /// </summary>
    /// <param name="phoneNumberOrEmail">手机号码</param>
    /// <param name="phoneNumberRegionCode"></param>
    /// <param name="requestType">验证码请求类型</param>
    /// <param name="type">验证码类型</param>
    /// <returns></returns>
    Task<DateTimeOffset?> GetLastSendSmsTime(
        string phoneNumberOrEmail,
        string? phoneNumberRegionCode,
        SmsCodeType? requestType = null,
        AuthMessageType? type = null);

    /// <summary>
    /// 根据手机号获取今天是否超过了最大的短信次数限制
    /// </summary>
    /// <param name="phoneNumber">手机号码</param>
    /// <param name="phoneNumberRegionCode"></param>
    /// <param name="maxSendSmsDay">一天内最大发送次数</param>
    /// <param name="type">验证码类型</param>
    /// <returns></returns>
    Task<bool> IsMaxSendSmsDay(
        string phoneNumber,
        string? phoneNumberRegionCode,
        byte? maxSendSmsDay = null,
        AuthMessageType? type = null);
}

partial interface IAuthMessageRecordRepository // 管理后台
{
    ///// <summary>
    ///// 后台表格查询
    ///// </summary>
    ///// <param name="userId">用户 Id</param>
    ///// <param name="phoneNumber">电话</param>
    ///// <param name="phoneNumberRegionCode"></param>
    ///// <param name="nickName"></param>
    ///// <param name="creationTime">创建时间</param>
    ///// <param name="email">邮箱地址</param>
    ///// <param name="requestType">短信验证用途</param>
    ///// <param name="everCheck">是否校验过</param>
    ///// <param name="checkSuccess"></param>
    ///// <param name="orderBy">排序字段</param>
    ///// <param name="desc">排序: <see langword="false"/> 为降序，<see langword="true"/> 为升序</param>
    ///// <param name="current"></param>
    ///// <param name="pageSize"></param>
    //Task<PagedModel<dynamic>> QueryAsync(
    //    Guid? userId,
    //    string? phoneNumber,
    //    string? phoneNumberRegionCode,
    //    string? nickName,
    //    DateTimeOffset?[]? creationTime,
    //    string? email,
    //    SmsCodeType? requestType,
    //    bool? everCheck,
    //    bool? checkSuccess,
    //    string? orderBy,
    //    bool? desc,
    //    int current = IPagedModel.DefaultCurrent,
    //    int pageSize = IPagedModel.DefaultPageSize);
}