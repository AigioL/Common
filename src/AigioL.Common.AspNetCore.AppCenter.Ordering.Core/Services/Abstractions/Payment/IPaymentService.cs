using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Payment;

public partial interface IPaymentService
{
    Task NotifyOrderClose(Guid id);

    Task NotifyOrderComplete(string orderNumber, string tradeNo, PaymentType paymentType, decimal amountReceived, DateTimeOffset paymentTime);
}