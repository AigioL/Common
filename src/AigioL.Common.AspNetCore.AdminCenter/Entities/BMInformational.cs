using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.AdminCenter.Entities;

/// <summary>
/// 管理后台信息表实体
/// </summary>
[Table("BMInformationals")]
public partial class BMInformational :
    Entity<Guid>,
    INEWSEQUENTIALID,
    ITenantId
{
    /// <inheritdoc/>
    [Comment("租户 Id")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// 管理后台网站名称
    /// </summary>
    [Required]
    [Comment("网站名称")]
    [StringLength(MaxLengths.LongName)]
    public required string WebsiteName { get; set; }

    /// <summary>
    /// 管理后台网站域名
    /// </summary>
    [Required]
    [Comment("网站域名")]
    [StringLength(MaxLengths.Url)]
    public required string WebsiteDomainName { get; set; }
}
