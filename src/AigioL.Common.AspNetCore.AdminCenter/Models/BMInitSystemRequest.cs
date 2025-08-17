namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 初始化管理后台系统请求
/// </summary>
/// <param name="TenantId">租户 Id</param>
/// <param name="TenantName">租户名称</param>
/// <param name="UserName">用户名</param>
/// <param name="Password">密码</param>
/// <param name="InitPassword">初始化密码</param>
public sealed record BMInitSystemRequest(string TenantId, string TenantName, string UserName, string Password, string InitPassword);