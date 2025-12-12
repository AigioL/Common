using AigioL.Common.AspNetCore.AppCenter.Ordering.Data.Abstractions;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Payment;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Payment;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Payment;

sealed partial class MerchantDeductionAgreementRepository<TDbContext> :
    Repository<TDbContext, MerchantDeductionAgreement, Guid>,
    IMerchantDeductionAgreementRepository
    where TDbContext : DbContext, IPaymentDbContext
{
    public MerchantDeductionAgreementRepository(TDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
    {
    }

    public async Task<MerchantDeductionAgreement?> GetByNo(string agreementNo, CancellationToken cancellationToken = default)
    {
        var r = await db.MerchantDeductionAgreements.AsNoTrackingWithIdentityResolution()
             .Where(x => x.AgreementNo == agreementNo)
             .FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<bool> DoUnSignAgreement(string agreementNo)
    {
        var r = await db.MerchantDeductionAgreements
            .Where(a => a.AgreementNo == agreementNo)
            .ExecuteUpdateAsync(a => a.SetProperty(b => b.Status, AgreementStatus.Terminating));
        return r > 0;
    }

    public async Task CompleteAgreementSign(MerchantDeductionAgreement merchantDeductionAgreement)
    {
        await db.MerchantDeductionAgreements
            .Where(a => a.AgreementNo == merchantDeductionAgreement.AgreementNo)
            .ExecuteUpdateAsync(a => a
                .SetProperty(b => b.ExtAgreementNo, b => merchantDeductionAgreement.ExtAgreementNo)
                .SetProperty(b => b.SigningTime, b => merchantDeductionAgreement.SigningTime)
                .SetProperty(b => b.UserOpenId, b => merchantDeductionAgreement.UserOpenId)
                .SetProperty(b => b.UserLoginAccount, b => merchantDeductionAgreement.UserLoginAccount)
                .SetProperty(b => b.ValidTime, b => merchantDeductionAgreement.ValidTime)
                .SetProperty(b => b.InvalidTime, b => merchantDeductionAgreement.InvalidTime)
                .SetProperty(b => b.Status, b => AgreementStatus.Signed)
                .SetProperty(b => b.NoticeStatus, NoticeStatus.WaitNotice)
                .SetProperty(b => b.NoticeCount, 0)
                .SetProperty(b => b.NoticeFinishTime, (DateTimeOffset?)null)
            );
    }

    public async Task CompleteAgreementUnSign(string agreementNo, DateTimeOffset unSigningTime)
    {
        await db.MerchantDeductionAgreements
            .Where(a => a.AgreementNo == agreementNo)
            .ExecuteUpdateAsync(a => a
                .SetProperty(b => b.UnSigningTime, b => unSigningTime)
                .SetProperty(b => b.UpdateTime, DateTimeOffset.Now)
                .SetProperty(b => b.Status, b => AgreementStatus.Terminated)
                .SetProperty(b => b.NoticeStatus, NoticeStatus.WaitNotice)
                .SetProperty(b => b.NoticeCount, 0)
                .SetProperty(b => b.NoticeFinishTime, (DateTimeOffset?)null)
            );
    }

    public async Task UpdateNextDeductionTime(MerchantDeductionAgreement agreement, DateTimeOffset paymentTime)
    {
        var updatedNextDeductionTime = agreement.PeriodType switch
        {
            "DAY" => (DateTimeOffset)(agreement.NextDeductionTime ?? paymentTime).Date.AddDays(agreement.Period),
            _ => throw new ArgumentOutOfRangeException(nameof(agreement.PeriodType), agreement.PeriodType, null)
        };

        var r = await db.MerchantDeductionAgreements
            .Where(a => a.AgreementNo == agreement.AgreementNo)
            .ExecuteUpdateAsync(a => a
                .SetProperty(b => b.NextDeductionTime, updatedNextDeductionTime)
                .SetProperty(b => b.UpdateTime, DateTimeOffset.Now));
    }

    public async Task<Order?> GetFirstDeductionOrderToAgreement(string agreementNo, CancellationToken cancellationToken = default)
    {
        var query = db.Orders.AsNoTrackingWithIdentityResolution()
            .Include(a => a.MerchantDeductionAgreement)
            .Include(a => a.PaymentCompositions!
                .Where(b => b.PaymentMethod == PaymentMethod.Online
                    && b.PaymentStatus == PaymentStatus.WaitPay))
            .Where(a => a.MerchantDeductionAgreement!.AgreementNo == agreementNo);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<List<MerchantDeductionAgreement>> GetAgreementsForDeduction(int daysInAdvance = 0, DateTimeOffset? debugDate = null, CancellationToken cancellationToken = default)
    {
        var start = debugDate ?? DateTimeOffset.Now.Date;
        var end = (debugDate ?? start).AddDays(daysInAdvance);

        var query = db.MerchantDeductionAgreements
            .AsNoTrackingWithIdentityResolution()
            //.Include(a => a.XunYouMDAExtend)
            // 已签约
            .Where(a => a.Status == AgreementStatus.Signed)
            // 接近下次扣款时间
            .Where(a => start <= a.NextDeductionTime && a.NextDeductionTime <= end)
            // 排除已经创建本次扣款订单的协议
            .Where(agree => !agree.Orders!.Any(order => order.Timeout >= agree.NextDeductionTime!.Value.AddDays(1)));

        var r = await query.ToListAsync(cancellationToken);
        return r;
    }

    public async Task<List<AgreementModel>> GetAgreementsByUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = db.MerchantDeductionAgreements
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.UserId == userId)
            .Where(a => a.Status != AgreementStatus.UnSigned)
            .OrderByDescending(a => a.CreationTime)
            .Select(ProjectToMapper.AgreementModelExpr);

        var r = await query.ToListAsync(cancellationToken);
        return r;
    }

    public async Task<MerchantDeductionAgreement?> GetAgreementAndOrdersByNo(string agreementNo, CancellationToken cancellationToken = default)
    {
        var query = db.MerchantDeductionAgreements
            .AsNoTrackingWithIdentityResolution()
            .Include(a => a.Orders)
            .Where(a => a.AgreementNo == agreementNo);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<MerchantDeductionAgreementConfiguration?> GetConfiguration(string configurationCode, CancellationToken cancellationToken = default)
    {
        var query = db.MerchantDeductionAgreementConfigurations
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.Code == configurationCode);

        var r = await query.FirstOrDefaultAsync(cancellationToken);
        return r;
    }

    public async Task<bool> CheckUserBusinessSigned(Guid userId, int businessType, CancellationToken cancellationToken = default)
    {
        var query = db.MerchantDeductionAgreements
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.Status == AgreementStatus.Signed)
            .Where(a => a.UserId == userId && a.BusinessTypeId == businessType);

        var r = await query.AnyAsync(cancellationToken);
        return r;
    }

    public Task UpdateNoticeCount(Guid id, int noticeCount)
    {
        return db.MerchantDeductionAgreements.Where(a => a.Id == id)
            .ExecuteUpdateAsync(p => p.SetProperty(f => f.NoticeCount, f => noticeCount));
    }

    public async Task<List<MerchantDeductionAgreement>> GetMerchantAgreementOfDeductionTimeout(CancellationToken cancellationToken = default)
    {
        var query = db.MerchantDeductionAgreements
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.Status == AgreementStatus.Signed)
            .Where(a => a.Orders!.Any(o => o.Status == OrderStatus.Expired && o.Timeout >= a.NextDeductionTime!.Value.AddDays(1)));

        var r = await query.ToListAsync(cancellationToken);
        return r;
    }
}

file static class ProjectToMapper
{
    internal static readonly Expression<Func<MerchantDeductionAgreement, AgreementModel>> AgreementModelExpr = it => new()
    {
        Id = it.Id,
        SigningTime = it.SigningTime,
        UnSigningTime = it.UnSigningTime,
        Platform = it.Platform,
        AgreementNo = it.AgreementNo,
        ExtAgreementNo = it.ExtAgreementNo,
        ValidTime = it.ValidTime,
        InvalidTime = it.InvalidTime,
        Period = it.Period,
        PeriodType = it.PeriodType,
        ExecuteTime = it.ExecuteTime,
        NextDeductionTime = it.NextDeductionTime,
        SingleAmount = it.SingleAmount,
        Status = it.Status,
        Note = it.Note,
        BusinessType = it.BusinessTypeId,
    };
}