using System.ComponentModel.DataAnnotations;

namespace AigioL.Common.AspNetCore.AdminCenter.PartnerCenter.Models;

/// <summary>
/// 更换 PC 用户微信 OpenId 请求模型
/// </summary>
public sealed class ChangePCUserWeChatOpenIdModel
{
    /// <summary>
    /// 微信 OpenId
    /// </summary>
    [Required(ErrorMessage = "微信 OpenId 不能为空")]
    public string OpenId { get; set; } = null!;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [Required(ErrorMessage = "真实姓名不能为空")]
    public string RealName { get; set; } = null!;
}
