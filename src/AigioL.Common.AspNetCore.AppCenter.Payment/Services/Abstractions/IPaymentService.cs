using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Services.Abstractions;

public partial interface IPaymentService
{
    Task NotifyOrderClose(Guid paymentId);

    Task NotifyOrderComplete(string orderNumber, string tradeNo, PaymentType paymentType, decimal amountReceived, DateTimeOffset paymentTime);
}