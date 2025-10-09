namespace AigioL.Common.AspNetCore.AppCenter.Security;

public partial interface ISecurityKeyOptions
{
    /// <summary>
    /// 共享密钥
    /// </summary>
    byte[]? ECDH_SharedKey { get; set; }
}
