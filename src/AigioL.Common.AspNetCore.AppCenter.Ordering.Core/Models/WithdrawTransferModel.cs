using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public record class WithdrawTransferModel(
#if !USE_NUM_UID
    Guid UserId,
#else
    long UserId,
#endif
    string TransferNumber,
    string Title,
    decimal TransferAmount,
    string UserOpenId,
    string? UserLoginAccount,
    PaymentType PaymentPlatform);