using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Payment.Data.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Data.Abstractions
{
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
}

#if PROJ_DBCONTEXT_BM
namespace GameTrainer.ApiService.AdminCenter.Data
{
    partial class BMDbContext : IPaymentDbContext
    {
        public DbSet<OrderBusinessPaymentConfiguration> OrderBusinessPaymentConfigurations { get; set; } = null!;

        public DbSet<CooperatorAccount> CooperatorAccounts { get; set; } = null!;

        public DbSet<MerchantDeductionAgreement> MerchantDeductionAgreements { get; set; } = null!;

        public DbSet<MerchantDeductionAgreementConfiguration> MerchantDeductionAgreementConfigurations { get; set; } = null!;

        public DbSet<MembershipBusinessOrder> MembershipBusinessOrders { get; set; } = null!;

        public DbSet<MembershipGoods> MembershipGoods { get; set; } = null!;

        public DbSet<MembershipGoodsMDARelation> MembershipGoodsMDARelations { get; set; } = null!;

        public DbSet<MembershipProductKeyRecord> MembershipProductKeyRecords { get; set; } = null!;

        public DbSet<MembershipGoodsUserFirstRecord> MembershipGoodsUserFirstRecords { get; set; } = null!;
    }
}
#endif