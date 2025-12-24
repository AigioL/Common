using AigioL.Common.AspNetCore.AppCenter.Models;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

/// <summary>
/// 验证码发送记录表格项
/// </summary>
public sealed partial record class AuthMessageRecordTableItem : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// IP 地址
    /// </summary>
    public string IPAddress { get; set; } = string.Empty;

    /// <summary>
    /// 第三方下发渠道的显示名称
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// 第三方提供商返回的内容
    /// </summary>
    public string? SendResultRecord { get; set; }

    /// <summary>
    /// 第三方提供商返回的 HTTP 状态码
    /// </summary>
    public int HttpStatusCode { get; set; }

    /// <summary>
    /// 第三方提供商发送是否成功
    /// </summary>
    public bool SendIsSuccess { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 是否已经校验过
    /// </summary>
    public bool EverCheck { get; set; }

    /// <summary>
    /// 是否校验成功
    /// </summary>
    public bool CheckSuccess { get; set; }

    /// <summary>
    /// 是否被废弃
    /// </summary>
    public bool Abandoned { get; set; }

    /// <summary>
    /// 校验失败次数
    /// </summary>
    public int CheckFailuresCount { get; set; }

    /// <summary>
    /// 是属于邮箱验证还是短信验证
    /// </summary>
    public AuthMessageType Type { get; set; }

    /// <summary>
    /// 发送验证码用途
    /// </summary>
    public SmsCodeType RequestType { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfoModel? UserInfo { get; set; }
}
