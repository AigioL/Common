namespace AigioL.Common.AspNetCore.AppCenter.Security;

/// <summary>
/// SecurityKey 模式密钥算法类型
/// </summary>
public enum SecurityKeyAlgorithmType : byte
{
    /// <summary>
    /// App 版本号固定 RSA 私钥，公钥写入客户端程序，随机生成 AES 密钥和 IV，使用 RSA 加密 AES 密钥（写入请求头），使用 AES 加密数据
    /// </summary>
    RSAWithRandomAes = 1,

    /// <summary>
    /// 使用 AppSettings 中的 ECDH_SharedKey 作为 AES 密钥，随机生成 IV，IV 值在请求头中，使用 AES 加密数据
    /// </summary>
    ECDHSharedKeyWithRandomIV = 2,
}
