namespace AigioL.Common.AspNetCore.AdminCenter.Models;

/// <summary>
/// 初始化后台管理系统请求
/// </summary>
/// <param name="TenantId">租户 Id</param>
/// <param name="TenantName">租户名称</param>
/// <param name="UserName">用户名</param>
/// <param name="Password">密码</param>
/// <param name="InitPassword">初始化密码</param>
public sealed record InitSystemRequest(string TenantId, string TenantName, string UserName, string Password, string InitPassword);