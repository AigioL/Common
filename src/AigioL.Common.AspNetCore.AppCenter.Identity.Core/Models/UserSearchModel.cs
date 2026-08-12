using AigioL.Common.Primitives.Columns;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserSearchModel
{
    public int Count { get; set; }

    public List<UserSearchItemModel> Items { get; set; } = new();
}

public sealed partial record class UserSearchItemModel
{
#if !USE_NUM_UID
    public Guid Id { get; set; }
#else
    public long Id { get; set; }
#endif

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

#if !USE_NUM_UID
partial record class UserSearchItemModel : IReadOnlyId<Guid>;
#else
partial record class UserSearchItemModel : IReadOnlyId<long>;
#endif