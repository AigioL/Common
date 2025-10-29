using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;

public interface IPaymentDbContext : IOrderingPaymentBaseDbContext, IDbContextBase
{
    DbSet<OrderBusinessPaymentConfiguration> OrderBusinessPaymentConfigurations { get; }

    DbSet<CooperatorAccount> CooperatorAccounts { get; }

    DbSet<MerchantDeductionAgreement> MerchantDeductionAgreements { get; }

    DbSet<MerchantDeductionAgreementConfiguration> MerchantDeductionAgreementConfigurations { get; }

    DbSet<MembershipBusinessOrder> MembershipBusinessOrders { get; }

    DbSet<MembershipGoods> MembershipGoods { get; }

    DbSet<MembershipGoodsMDARelation> MembershipGoodsMDARelations { get; }

    DbSet<MembershipProductKeyRecord> MembershipProductKeyRecords { get; }

    DbSet<MembershipGoodsUserFirstRecord> MembershipGoodsUserFirstRecords { get; }
}