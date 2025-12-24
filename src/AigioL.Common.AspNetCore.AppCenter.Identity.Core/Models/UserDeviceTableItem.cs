using AigioL.Common.Primitives.Columns;
using AigioL.Common.Primitives.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Identity.Models;

public sealed partial record class UserDeviceTableItem : IReadOnlyId<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// 用户 Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = "";

    /// <summary>
    /// 设备唯一识别码
    /// </summary>
    public string DeviceId { get; set; } = "";

    /// <summary>
    /// 上次登录时间
    /// </summary>
    public DateTimeOffset LastLoginTime { get; set; }

    /// <summary>
    /// 是否信任
    /// </summary>
    public bool IsTrust { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreateTime { get; set; }

    /// <summary>
    /// 设备所属终端
    /// </summary>
    public DevicePlatform2 Platform { get; set; }

    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfoModel? UserInfo { get; set; }
}
