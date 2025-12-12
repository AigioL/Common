using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

sealed partial class PaymentMessageQueueService : IPaymentMessageQueueService
{
    public Task PushPaymentSuccess(OrderPaymentSuccessInfo info)
    {
        throw new NotImplementedException();
    }

    public Task PushRefundSuccess(OrderRefundSuccessInfo info)
    {
        throw new NotImplementedException();
    }

    public Task PushSignAgreementSuccess(string agreementNo)
    {
        throw new NotImplementedException();
    }

    public Task PushUnSignAgreementSuccess(string agreementNo)
    {
        throw new NotImplementedException();
    }
}
