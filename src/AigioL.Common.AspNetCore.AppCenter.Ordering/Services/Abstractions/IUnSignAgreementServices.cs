using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;

/// <summary>
/// 解约委托代扣协议服务
/// </summary>
public interface IUnSignAgreementServices
{
    /// <summary>
    /// 解约委托代扣协议
    /// </summary>
    /// <param name="extAgreementNo">外部协议号</param>
    /// <returns></returns>
    Task<bool> UnSignAgreement(string extAgreementNo);

    /// <summary>
    /// 根据支付类型获取解约服务实例
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="paymentType"></param>
    /// <returns></returns>
    static IUnSignAgreementServices? GetServices(IServiceProvider serviceProvider, PaymentType paymentType) => paymentType switch
    {
        PaymentType.Alipay => serviceProvider.GetRequiredService<IAliPayServices>(),
        PaymentType.WeChatPay => serviceProvider.GetRequiredKeyedService<IWeChatPayServices>(IWeChatPayServices.V2),
        //PaymentType.UnionPay => TODO: 银联解约
        _ => null,
    };
}
