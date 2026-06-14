using AigioL.Common.AspNetCore.AdminCenter.Models;
using Microsoft.AspNetCore.Authorization;

namespace AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;

/// <summary>
/// 权限授权要求类，用于定义控制器的权限授权需求
/// </summary>
/// <param name="controllerName"></param>
/// <param name="buttonType"></param>
public sealed record class PermissionAuthorizationRequirement(string controllerName, BMButtonType buttonType) : PermissionAuthorizationRequirement<BMButtonType>(controllerName, buttonType)
{
    public static implicit operator AuthorizationPolicy(PermissionAuthorizationRequirement obj) => obj.GetAuthorizationPolicy();
}

public abstract record class PermissionAuthorizationRequirementBase : IAuthorizationRequirement
{
    /// <summary>
    /// 控制器名称
    /// </summary>
    public abstract string ControllerName { get; }

    public abstract string ButtonTypeString { get; }

    public AuthorizationPolicy GetAuthorizationPolicy() => new([this], [BMMinimalApis.BearerScheme]);

    public static string GetPolicyName(string controllerName, string buttonType) => $"{controllerName}{buttonType}";

    public string GetPolicyName() => GetPolicyName(ControllerName, ButtonTypeString);
}

public abstract record class PermissionAuthorizationRequirement<TButtonType>(string controllerName, TButtonType buttonType) : PermissionAuthorizationRequirementBase where TButtonType : struct, Enum
{
    public override string ControllerName => controllerName;

    /// <summary>
    /// 按钮类型
    /// </summary>
    public TButtonType ButtonType => buttonType;

    static string GetButtonTypeString(TButtonType buttonType) => buttonType.ToString()!;

    public override string ButtonTypeString => GetButtonTypeString(buttonType);

    public static string GetPolicyName(string controllerName, TButtonType buttonType) => GetPolicyName(controllerName, GetButtonTypeString(buttonType));
}