using Essensoft.Paylink.WeChatPay;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

public sealed partial class WeChatPayExOptions : WeChatPayOptions
{
    /// <summary>
    /// 微信支付回调通知
    /// </summary>
    public string NotifyUrl
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }

    /// <summary>
    /// 微信退款回调通知
    /// </summary>
    public string RefundNotifyUrl
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }

    /// <summary>
    /// 微信签约信息回调通知
    /// </summary>
    public string ContractNotifyUrl
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }
}
