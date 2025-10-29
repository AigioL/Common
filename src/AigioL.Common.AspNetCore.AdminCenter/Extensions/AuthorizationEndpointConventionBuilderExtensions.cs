using AigioL.Common.AspNetCore.AdminCenter.Models;
using AigioL.Common.AspNetCore.AdminCenter.Policies.Requirements;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.AspNetCore.Builder;

public static partial class AuthorizationEndpointConventionBuilderExtensions
{
    /// <summary>
    /// 配置权限过滤器
    /// </summary>
    public static TBuilder PermissionFilter<TBuilder>(this TBuilder builder, string controllerName, BMButtonType buttonType = default)
        where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(new PermissionAuthorizationRequirement(controllerName, buttonType));
}
