using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models;

public partial record class IdNameModel : IReadOnlyId<Guid>
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = "";
}

public partial record class IdNameDescriptionModel : IdNameModel
{
    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

#if DEBUG
[Obsolete("use IdNameModel", true)]
public record OptionItem : IdNameModel { }

[Obsolete("use IdNameDescriptionModel", true)]
public record OptionItemWithDescription : IdNameDescriptionModel { }
#endif