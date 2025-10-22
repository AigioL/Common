using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.ApiGateway.Entities;

/// <summary>
/// 用于 Api 网关的 Yarp 反向代理配置表实体类
/// </summary>
[Table("YarpReverseProxyConfigs")]
public sealed partial class YarpReverseProxyConfig : Entity<Guid>, IDisable, ISoftDeleted
{
    /// <summary>
    /// 将匹配的请求路由到集群
    /// </summary>
    [Comment("将匹配的请求路由到集群")]
    [Column(TypeName = "jsonb")] // https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Ccomplex-types%2Cjsondocument
    public required string Routes { get; set; }

    /// <summary>
    /// 有关将请求代理到何处的集群信息
    /// </summary>
    [Comment("有关将请求代理到何处的集群信息")]
    [Column(TypeName = "jsonb")] // https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Ccomplex-types%2Cjsondocument
    public required string Clusters { get; set; }

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }
}