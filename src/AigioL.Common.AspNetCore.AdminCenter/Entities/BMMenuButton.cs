using AigioL.Common.Primitives.Columns;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <summary>
/// 管理后台的菜单与按钮关联表实体
/// </summary>
[Table("BMMenuButtons")]
public partial class BMMenuButton :
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

    /// <inheritdoc cref="BMButton"/>
    public BMButton? Button { get; set; }

    /// <inheritdoc cref="BMMenu"/>
    public BMMenu? Menu { get; set; }

    /// <inheritdoc/>
    [Comment("并发令牌")]
    public uint RowVersion { get; set; }
}
