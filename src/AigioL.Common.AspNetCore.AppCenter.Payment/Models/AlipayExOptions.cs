using Essensoft.Paylink.Alipay;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

public sealed partial class AlipayExOptions : AlipayOptions
{
    /// <summary>
    /// 支付宝支付回调通知
    /// </summary>
    public string NotifyUrl
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set => field = value;
    }

    /// <summary>
    /// 支付宝支付成功后跳转
    /// </summary>
    public string ReturnUrl
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set => field = value;
    }
}
