using AigioL.Common.AspNetCore.AdminCenter.Models;
using Microsoft.AspNetCore.Authorization;

namespace AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;

/// <summary>
/// 权限授权要求类，用于定义控制器的权限授权需求
/// </summary>
/// <param name="controllerName"></param>
/// <param name="buttonType"></param>
public sealed record class PermissionAuthorizationRequirement(string controllerName, BMButtonType buttonType) : IAuthorizationRequirement
{
    /// <summary>
    /// 控制器名称
    /// </summary>
    public string ControllerName => controllerName;

    /// <summary>
    /// 按钮类型
    /// </summary>
    public BMButtonType ButtonType => buttonType;

    public AuthorizationPolicy GetAuthorizationPolicy() => new([this], [BMMinimalApis.BearerScheme]);

    public static implicit operator AuthorizationPolicy(PermissionAuthorizationRequirement obj) => obj.GetAuthorizationPolicy();

    public static string GetPolicyName(string controllerName, BMButtonType buttonType) => $"{controllerName}{buttonType}";

    public string GetPolicyName() => GetPolicyName(controllerName, buttonType);
}
