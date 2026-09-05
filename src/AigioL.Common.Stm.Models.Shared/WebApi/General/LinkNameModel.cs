namespace AigioL.Common.Stm.Models.WebApi.General;

/// <summary>
/// 通用模型
/// {"link":TLink,"name":TName}
/// </summary>
public partial record class LinkNameModel<TLink, TName>
{
    public TLink? Link { get; set; }

    public TName? Name { get; set; }
}

/// <summary>
/// 通用模型
/// {"link":null,"name":null}
/// </summary>
public sealed partial record class LinkNameModel : LinkNameModel<string?, string?>;