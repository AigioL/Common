namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Abstractions;

/// <summary>
/// 第三方外部账号登录终结点相关的跳转地址配置项
/// </summary>
public interface IExternalLoginRedirect
{
    /// <summary>
    /// 账号中心 - 绑定（跳转地址）
    /// <para>默认值：/account/center/bind</para>
    /// </summary>
    string? AccountCenterBindUrl { get; }

    /// <summary>
    /// 账号登录（跳转地址）
    /// <para>默认值：/account/login</para>
    /// </summary>
    string? AccountLoginUrl { get; set; }
}
