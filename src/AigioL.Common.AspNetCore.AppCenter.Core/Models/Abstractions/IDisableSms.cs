namespace AigioL.Common.AspNetCore.AppCenter.Models.Abstractions;

public partial interface IDisableSms
{
    /// <summary>
    /// 禁用短信验证码
    /// </summary>
    bool DisableSms { get; }
}
