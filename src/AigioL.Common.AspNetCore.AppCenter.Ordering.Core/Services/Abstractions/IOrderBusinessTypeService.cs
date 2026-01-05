namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Services.Abstractions;

/// <summary>
/// 业务订单类型服务接口
/// </summary>
public partial interface IOrderBusinessTypeService
{
    /// <summary>
    /// 获取会员业务类型值
    /// </summary>
    int Membership { get; }
}
