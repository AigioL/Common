using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;
using System.ComponentModel.DataAnnotations;
using RModelValid = AigioL.Common.AspNetCore.AppCenter.Properties.ModelValidationErrors;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models.Request;

/// <summary>
/// 编辑用户（个人资料/我的资料）请求模型，使用 VersionTolerant 以支持向后兼容
/// </summary>
[global::MemoryPack.MemoryPackable(global::MemoryPack.GenerateType.VersionTolerant, global::MemoryPack.SerializeLayout.Explicit)]
public sealed partial record class EditUserProfileRequest : IValidatableObject
{
    /// <summary>
    /// 昵称
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(0)]
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 头像
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(1)]
    public Guid? Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(2)]
    public Gender Gender { get; set; }

    /// <summary>
    /// 出生日期
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(3)]
    public DateTime? BirthDate { get; set; }

    [global::MemoryPack.MemoryPackOrder(4)]
    public sbyte BirthDateTimeZone { get; set; }

    /// <summary>
    /// 地区
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(5)]
    public int? AreaId { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    [global::MemoryPack.MemoryPackOrder(6)]
    public string? PersonalizedSignature { get; set; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(NickName))
        {
            yield return new ValidationResult(RModelValid.请输入昵称, [nameof(NickName)]);
        }
        if (NickName.Length > MaxLengths.CUserNickName)
        {
            yield return new ValidationResult(RModelValid.昵称最大长度不能超过_.Format(MaxLengths.CUserNickName), [nameof(NickName)]);
        }
    }
}
