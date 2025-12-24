using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserSearchModel
{
    public int Count { get; set; }

    public List<UserSearchItemModel> Items { get; set; } = new();
}

public sealed partial record class UserSearchItemModel : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 手机号码
    /// </summary>
    public string? PhoneNumber { get; set; }

    public string? PhoneNumberRegionCode { get; set; }

    /// <summary>
    /// 头像 Url
    /// </summary>
    public string? AvatarUrl { get; set; }
}