namespace AigioL.Common.Stm.Models.WebApi.General;

/// <summary>
/// 通用模型
/// {"type":TType,"value":TValue}
/// </summary>
public partial record class TypeValueModel<TType, TValue>
{
    /// <summary>
    /// 类型
    /// </summary>
    public TType? Type { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public TValue? Value { get; set; }
}

/// <summary>
/// 通用模型
/// {"type":null,"value":null}
/// </summary>
public sealed partial record class TypeValueModel : TypeValueModel<string?, string?>;