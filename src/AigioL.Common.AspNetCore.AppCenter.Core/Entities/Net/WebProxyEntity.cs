using AigioL.Common.AspNetCore.AppCenter.Models.Net;
using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AigioL.Common.AspNetCore.AppCenter.Entities.Net;

public partial class WebProxyEntity :
    IEntity<string>,
    IDisable,
    INote
{
    [Comment("主键")]
    [StringLength(MaxLengths.Url)]
    public required string Id { get; set; }

    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <summary>
    /// 获取或设置代理服务器的地址
    /// </summary>
    [StringLength(MaxLengths.Url)]
    public string? Address { get; set; }

    /// <summary>
    /// 获取或设置一个地址数组，这些地址不使用代理服务器
    /// </summary>
    public string[]? BypassList { get; set; } // PostgreSQL 数组列 https://devblogs.microsoft.com/dotnet/array-mapping-in-ef-core-8/#postgresql-array-columns

    /// <summary>
    /// 获取或设置一个值，该值指示是否跳过代理服务器而使用本地地址
    /// </summary>
    public bool BypassProxyOnLocal { get; set; }

    /// <summary>
    /// 获取或设置 <see cref="bool"/> 值，该值控制 <see cref="CredentialCache.DefaultCredentials "/> 是否随请求一起发送
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    /// <inheritdoc cref="NetworkCredential.Domain"/>
    [StringLength(MaxLengths.Url)]
    public string? Domain { get; set; }

    /// <inheritdoc cref="NetworkCredential.Password"/>
    [StringLength(MaxLengths.Url)]
    public string? Password { get; set; }

    /// <inheritdoc cref="NetworkCredential.UserName"/>
    [StringLength(MaxLengths.Url)]
    public string? UserName { get; set; }

    public WebProxyCredentialsType CredentialsType { get; set; }

    /// <summary>
    /// 组 Id，由业务自行定义分组以应用于不同用途的业务场景
    /// </summary>
    public byte GroupId { get; set; }

    [Comment("备注")]
    [StringLength(MaxLengths.Text)]
    public string? Note { get; set; }

    public WebProxy GetWebProxy()
    {
        var credentials = CredentialsType switch
        {
            WebProxyCredentialsType.Null => null,
            WebProxyCredentialsType.NetworkCredential => new NetworkCredential(UserName, Password, Domain),
            _ => throw new ArgumentOutOfRangeException(nameof(CredentialsType), CredentialsType, null),
        };
        WebProxy p = new(Address, BypassProxyOnLocal, BypassList, credentials)
        {
            UseDefaultCredentials = UseDefaultCredentials,
        };
        return p;
    }
}
