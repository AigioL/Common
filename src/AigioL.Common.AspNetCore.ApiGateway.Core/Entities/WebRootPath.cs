using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AigioL.Common.AspNetCore.ApiGateway.Entities;

/// <summary>
/// Web 根路径表实体类
/// </summary>
[Table("WebRootPaths")]
public sealed partial class WebRootPath : Entity<short>, IDisable, ISoftDeleted
{
    /// <inheritdoc/>
    [Comment("匹配域名地址数组")]
    [Required]
    [StringLength(MaxLengths.Url)]
    [Column(TypeName = "jsonb")] // https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Ccomplex-types%2Cjsondocument
    public required string MatchDomainNames { get; set; }

    #region PhysicalFileProvider

    /// <summary>
    /// 相对路径
    /// </summary>
    [Comment("相对路径")]
    [StringLength(MaxLengths.FileName)]
    public string? RelativePath { get; set; }

    /// <summary>
    /// 指定文件或目录的筛选行为
    /// <para>https://learn.microsoft.com/zh-cn/dotnet/api/microsoft.extensions.fileproviders.physical.exclusionfilters</para>
    /// </summary>
    [Comment("排除筛选器")]
    public int? ExclusionFilters { get; set; }

    #endregion

    /// <inheritdoc/>
    [Comment("是否禁用")]
    public bool Disable { get; set; }

    /// <summary>
    /// 默认文档
    /// </summary>
    [Comment("默认文档")]
    [StringLength(MaxLengths.FileName)]
    public string? DefaultFile { get; set; }

    /// <inheritdoc/>
    [Comment("是否软删除")]
    public bool SoftDeleted { get; set; }
}

public partial class WebRootPath
{
    /// <summary>
    /// 默认文档的默认值
    /// </summary>
    public const string DefaultDefaultFile = "index.html";

    /// <summary>
    /// 清单根的相对路径的默认值
    /// </summary>
    public const string DefaultManifestEmbeddedRoot = "/";

    static string GetAbsolutePathCore(ReadOnlySpan<char> baseDir, ReadOnlySpan<char> relativePath)
    {
        Span<char> path = stackalloc char[MaxLengths.FileName];
        if (baseDir.EndsWith('/') || baseDir.EndsWith("\\"))
        {
            baseDir = baseDir[..^1];
        }
        baseDir.CopyTo(path);
        path[baseDir.Length] = Path.DirectorySeparatorChar;
        if (relativePath.StartsWith('/') || relativePath.StartsWith('\\'))
        {
            relativePath = relativePath[1..];
        }
        relativePath.CopyTo(path[..(baseDir.Length + 1)]);
        path = path[..(baseDir.Length + 1 + relativePath.Length)];
        char? replaceDirectorySeparatorChar = Path.DirectorySeparatorChar switch
        {
            '/' => '\\',
            '\\' => '/',
            _ => null,
        };
        if (replaceDirectorySeparatorChar.HasValue)
        {
            if (path.IndexOf(replaceDirectorySeparatorChar.Value) >= 0)
            {
                for (int i = 0; i < path.Length; i++)
                {
                    if (path[i] == replaceDirectorySeparatorChar.Value)
                    {
                        path[i] = Path.DirectorySeparatorChar;
                    }
                }
            }
        }
        else
        {
            if (path.IndexOf('/') >= 0 || path.IndexOf('\\') >= 0)
            {
                for (int i = 0; i < path.Length; i++)
                {
                    var it = path[i];
                    if (it == '/' || it == '\\')
                    {
                        path[i] = Path.DirectorySeparatorChar;
                    }
                }
            }
        }
        return new string(path);
    }

    /// <summary>
    /// 获取绝对路径
    /// </summary>
    /// <param name="baseDir"></param>
    /// <returns></returns>
    public string GetAbsolutePath(ReadOnlySpan<char> baseDir)
    {
        var relativePath = RelativePath;
        ArgumentNullException.ThrowIfNull(relativePath);
        var r = GetAbsolutePathCore(baseDir, relativePath);
        return r;
    }
}