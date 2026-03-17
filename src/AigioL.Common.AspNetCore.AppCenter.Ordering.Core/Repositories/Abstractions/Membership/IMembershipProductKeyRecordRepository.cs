using AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Membership;
using AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;
using AigioL.Common.Models;
using AigioL.Common.Primitives.Models;
using AigioL.Common.Primitives.Models.Abstractions;
using AigioL.Common.Repositories.Abstractions;
using AigioL.Common.Repositories.EntityFrameworkCore.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Repositories.Abstractions.Membership;

public partial interface IMembershipProductKeyRecordRepository : IRepository<MembershipProductKeyRecord, Guid>, IEFRepository
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

    Task<MembershipProductKeyRecord?> GetProductKeyRecord(Guid recordId, bool? disable, bool? isUsed, CancellationToken cancellationToken = default);
}

partial interface IMembershipProductKeyRecordRepository // 管理后台
{
    Task<PagedModel<MembershipProductKeyRecordTableItem>> QueryAsync(
        Guid? key = null,
        int? rechargeDays = null,
        Guid? membershipGoodsId = null,
        bool? isUsed = null,
        bool? disable = null,
        string? orderBy = null,
        bool? desc = null,
        int current = IPagedModel.DefaultCurrent,
        int pageSize = IPagedModel.DefaultPageSize,
        CancellationToken cancellationToken = default);

    Task<ApiRsp<string[]?>> BatchCreateProductKeyRecordAsync(
        Guid createUserId,
        Guid membershipGoodsId,
        uint count,
        CancellationToken cancellationToken = default);

    Task<int> BatchDisableProductKeyRecordAsync(
        Guid? operatorUserId,
        bool disable,
        params IEnumerable<string> keys);

    Task<int> BatchDisableProductKeyRecordAsync(
        Guid? operatorUserId,
        bool disable,
        params IEnumerable<Guid> keys);
}