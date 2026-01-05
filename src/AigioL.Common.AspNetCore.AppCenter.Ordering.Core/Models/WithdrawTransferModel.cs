using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public record class WithdrawTransferModel(
    Guid UserId,
    string TransferNumber,
    string Title,
    decimal TransferAmount,
    string UserOpenId,
    string? UserLoginAccount,
    PaymentType PaymentPlatform);