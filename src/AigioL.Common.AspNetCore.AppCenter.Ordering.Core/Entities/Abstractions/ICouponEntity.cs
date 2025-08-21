using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Entities.Abstractions;

/// <summary>
/// 优惠劵实体接口
/// </summary>
public interface ICouponEntity :
    ICoupon,
    ICreationTime,
    IOperatorUserId,
    IUpdateTime
{
}