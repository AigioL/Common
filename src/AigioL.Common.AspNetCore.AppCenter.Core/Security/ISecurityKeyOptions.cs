namespace AigioL.Common.AspNetCore.AppCenter.Security;

public partial interface ISecurityKeyOptions
{
    /// <summary>
    /// DiffieHellman 密钥交换解密形式使用的公钥
    /// </summary>
    byte[]? ECDiffieHellmanPublicKey { get; set; }
}
