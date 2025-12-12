using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Membership;

sealed partial class UserMembershipService : IUserMembershipService
{
    public Task<Guid?> CreateMembershipOrderAsync(Guid userId, MembershipGoods goods)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateMembershipOrderByCDKeyAsync(Guid userId, MembershipProductKeyRecord productKeyRecord, MembershipGoods goods)
    {
        throw new NotImplementedException();
    }

    public Task<Order?> CreateMembershipOrderByMerchantDeductionAsync(MerchantDeductionAgreement agreement)
    {
        throw new NotImplementedException();
    }

    public Task<bool> OrderPaymentCancelHandleAsync(Guid orderId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> OrderPaymentRefundedHandleAsync(Guid orderId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> OrderPaymentSuccessHandleAsync(Guid orderId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SignMerchantDeductionSuccessHandleAsync(string agreementNo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UnSignMerchantDeductionSuccessHandleAsync(string agreementNo)
    {
        throw new NotImplementedException();
    }
}
