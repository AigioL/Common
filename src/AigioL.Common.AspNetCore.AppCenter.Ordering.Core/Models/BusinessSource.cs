namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 业务来源
/// </summary>
public enum BusinessSource : byte
{
    普通订单 = 0,
    CDK激活 = 1,
    协议扣款 = 2,
}
