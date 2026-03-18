using AigioL.Common.Primitives.Columns;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.PartnerCenter.Entities;

/// <summary>
/// 合作伙伴后台的菜单与按钮关联表实体
/// </summary>
[Table("PCMenuButtons")]
public partial class PCMenuButton :
    ITenantId,
    ISort,
    IRowVersion
{
    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// 菜单 Id
    /// </summary>
    [Comment("菜单 Id")]
    public Guid MenuId { get; set; }

    /// <summary>
    /// 按钮 Id
    /// </summary>
    [Comment("按钮 Id")]
    public Guid ButtonId { get; set; }

    /// <inheritdoc/>
    [Comment("排序")]
    public long Sort { get; set; }

    /// <inheritdoc cref="PCButton"/>
    public PCButton? Button { get; set; }

    /// <inheritdoc cref="PCMenu"/>
    public PCMenu? Menu { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }
}
