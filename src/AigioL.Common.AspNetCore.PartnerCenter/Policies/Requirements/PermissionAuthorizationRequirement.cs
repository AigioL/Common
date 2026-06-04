using AigioL.Common.AspNetCore.PartnerCenter.Models;
using Microsoft.AspNetCore.Authorization;

namespace AigioL.Common.AspNetCore.PartnerCenter.Policies.Requirements;

/// <summary>
/// 权限授权要求类，用于定义控制器的权限授权需求
/// </summary>
/// <param name="controllerName"></param>
/// <param name="buttonType"></param>
public sealed record class PermissionAuthorizationRequirement(string controllerName, PCButtonType buttonType) : global::AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements.PermissionAuthorizationRequirement<PCButtonType>(controllerName, buttonType)
{
    public static implicit operator AuthorizationPolicy(PermissionAuthorizationRequirement obj) => obj.GetAuthorizationPolicy();
}