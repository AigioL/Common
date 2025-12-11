using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;

public interface IMembershipProductKeyRecordRepository : IRepository<MembershipProductKeyRecord, Guid>, IEFRepository
{
    //Task<(bool success, Guid[] keys, string msg)> BatchCreateProductKeyRecord(MembershipBatchCreateProductKeyRecordRequest request);

    //Task<bool> BatchDisableProductKeyRecord(MembershipBatchDisableProductKeyRecordRequest request);

    //Task<PagedModel<MembershipProductKeyRecordModel>> QueryAsync(
    //    int current = IPagedModel.DefaultCurrent,
    //    int pageSize = IPagedModel.DefaultPageSize,
    //    ShortGuid? key = null,
    //    int? rechargeDays = null,
    //    Guid? membershipGoodsId = null,
    //    bool? isUsed = null,
    //    bool? disable = null,
    //    string? orderBy = null,
    //    bool? desc = null);

    Task<MembershipProductKeyRecord?> GetProductKeyRecord(Guid recordId, bool? disable, bool? isUsed);
}