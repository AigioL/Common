using AigioL.Common.AspNetCore.AppCenter.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Identity.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionServiceExtensions
{
    public static IServiceCollection AddOrderingRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IOrderingDbContext
    {
        services.TryAddScoped<IAftersalesBillRepository, AftersalesBillRepository<TDbContext>>();
        services.TryAddScoped<IOrderRepository, OrderRepository<TDbContext>>();
        return services;
    }

    public static IServiceCollection AddPaymentRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IPaymentDbContext, IIdentityDbContext
    {
        services.TryAddScoped<IPaymentRepository, PaymentRepository<TDbContext>>();
        services.TryAddScoped<IMerchantDeductionAgreementRepository, MerchantDeductionAgreementRepository<TDbContext>>();
        AddMembershipRepositories<TDbContext>(services);
        return services;
    }

    public static IServiceCollection AddMembershipRepositories<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] TDbContext>(
        this IServiceCollection services)
        where TDbContext : DbContext, IPaymentDbContext, IIdentityDbContext
    {
        services.TryAddScoped<IUserMembershipRepository, UserMembershipRepository<TDbContext>>();
        //services.TryAddScoped<IUserMembershipChangeRecordRepository, UserMembershipChangeRecordRepository<TDbContext>>();
        services.TryAddScoped<IMembershipBusinessOrderRepository, MembershipBusinessOrderRepository<TDbContext>>();
        services.TryAddScoped<IMembershipGoodsRepository, MembershipGoodsRepository<TDbContext>>();
        services.TryAddScoped<IMembershipProductKeyRecordRepository, MembershipProductKeyRecordRepository<TDbContext>>();
        AddMembershipServices(services);
        return services;
    }

    public static IServiceCollection AddMembershipServices(
        this IServiceCollection services)
    {
        services.TryAddScoped<IUserMembershipService, UserMembershipService>();
        return services;
    }
}
