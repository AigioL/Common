namespace AigioL.Common.AspNetCore.AdminCenter.Constants;

/// <summary>
/// 多租户体系的常量
/// </summary>
public static partial class TenantConstants
{
    /// <summary>
    /// 根租户 Id 字符串
    /// </summary>
    public const string RootTenantId = "10000000000000000000000000000000";

    /// <summary>
    /// 根租户 Id
    /// </summary>
    public static Guid RootTenantIdG => _.RootTenantIdG;
}

file static class _
{
    internal static readonly Guid RootTenantIdG = Guid.ParseExact(TenantConstants.RootTenantId, "N");
}