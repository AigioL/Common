namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.ApiGateway;

/// <summary>
/// 服务器证书原始数据类型
/// </summary>
public enum ServerCertificateRawDataType : byte
{
    /// <summary>
    /// .pfx 格式
    /// </summary>
    PFX = 2,

    /// <summary>
    /// .pem 格式
    /// </summary>
    PEM = 3,
}