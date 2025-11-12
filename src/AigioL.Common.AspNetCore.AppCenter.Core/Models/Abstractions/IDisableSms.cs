namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

public partial interface IDisableSms
{
    /// <summary>
    /// 禁用短信验证码服务，停止发送短信
    /// </summary>
    bool DisableSms { get; }
}
