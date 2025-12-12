namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

/// <summary>
/// 用户签约商家捐款协议
/// </summary>
/// <param name="ExecuteTime">扣款时间</param>
/// <param name="AgreementPageUrl">签约页面地址</param>
public sealed record UserAgreement(string AgreementPageUrl, DateTimeOffset? ExecuteTime = null)
{
    public string? FailMessage { get; private set; }

    public string? TradeNo { get; set; }

    public static UserAgreement Error(string message) => new(string.Empty) { FailMessage = message };
}