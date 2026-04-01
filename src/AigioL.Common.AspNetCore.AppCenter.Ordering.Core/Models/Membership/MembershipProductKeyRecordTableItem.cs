using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models.Membership;

public sealed partial class MembershipProductKeyRecordTableItem : IReadOnlyId<Guid>, ICreateTime
{
    public Guid Id { get; set; }

    /// <summary>
    /// <see cref="Id"/> 的 Base58 编码值
    /// </summary>
    public string IdBase58
    {
        get
        {
            field ??= Base58Guid.Encode(Id);
            return field;
        }
    }

    /// <summary>
    /// 充值时间跨度
    /// </summary>
    public TimeSpan RechargeTimeSpan { get; set; }

    /// <summary>
    /// 按量付费的会员时长
    /// </summary>
    public TimeSpan PayAsYoGo { get; set; }

    /// <summary>
    /// 是否已使用
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// 分成 KOL 用户 Id
    /// </summary>
    public Guid? RevenueShareRecipientKolUserId { get; set; }

    /// <summary>
    /// 绑定的合作伙伴用户到期时间
    /// </summary>
    public DateTimeOffset BindPCUserExpireDate { get; set; }

    /// <summary>
    /// 分成比例
    /// </summary>
    public decimal RevenueSharePercentage { get; set; }

    /// <summary>
    /// 是否禁用
    /// </summary>
    public bool Disable { get; set; }

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
