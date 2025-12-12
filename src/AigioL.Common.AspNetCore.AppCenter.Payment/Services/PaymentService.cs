using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services;

sealed partial class PaymentService : IPaymentService
{
    public Task NotifyOrderClose(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task NotifyOrderComplete(string orderNumber, string tradeNo, PaymentType paymentType, decimal amountReceived, DateTimeOffset paymentTime)
    {
        throw new NotImplementedException();
    }
}
