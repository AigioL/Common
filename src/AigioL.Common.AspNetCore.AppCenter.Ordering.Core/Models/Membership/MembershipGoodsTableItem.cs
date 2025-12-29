using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Membership;
using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

public sealed partial class MembershipGoodsTableItem : IReadOnlyId<Guid>, ICreateTime
{
    public Guid Id { get; set; }

    /// <summary>
    /// 商品名称
    /// </summary>
    public required string GoodsName { get; set; }

    /// <summary>
    /// 商品编号
    /// </summary>
    public required string GoodsNo { get; set; }

    /// <summary>
    /// 会员订阅类型，跟随变更记录更新
    /// </summary>
    public MembershipLicenseFlags MemberLicenseType { get; set; }

    /// <summary>
    /// 充值天数
    /// </summary>
    public int RechargeDays { get; set; }

    /// <summary>
    /// 首充原价
    /// </summary>
    public decimal? FirstPrice { get; set; }

    /// <summary>
    /// 首充当前价格
    /// </summary>
    public decimal? FirstCurrentPrice { get; set; }

    /// <summary>
    /// 原价
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// 当前价格
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// 是否上架
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Note { get; set; }

    public List<Guid> Configurations { get; set; } = [];

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdateTime { get; set; }

    /// <summary>
    /// 创建人 UserId（创建此条目的后台管理员）
    /// </summary>
    public Guid? CreateUserId { get; set; }

    /// <summary>
    /// 创建人（创建此条目的后台管理员）
    /// </summary>
    public string? CreateUser { get; set; }

    /// <summary>
    /// 最后一次操作的人 UserId（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public Guid? OperatorUserId { get; set; }

    /// <summary>
    /// 最后一次操作的人（记录后台管理员禁用或启用或编辑该条的操作）
    /// </summary>
    public string? OperatorUser { get; set; }
}
