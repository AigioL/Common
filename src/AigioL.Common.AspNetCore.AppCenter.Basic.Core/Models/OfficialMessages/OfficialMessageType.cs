namespace AigioL.Common.AspNetCore.AppCenter.Basic.Models.OfficialMessages;

/// <summary>
/// 官方消息类型
/// </summary>
public enum OfficialMessageType
{
    Custom = 0,

    /// <summary>
    /// 官方公告
    /// </summary>
    OfficialMessage = 1,

    /// <summary>
    /// 优惠活动
    /// </summary>
    Promotions = 2,

    /// <summary>
    /// 最新消息
    /// </summary>
    News = 3,
}

/// <summary>
/// Enum 扩展 <see cref="OfficialMessageType"/>
/// </summary>
public static partial class OfficialMessageTypeEnumExtensions
{
    /// <summary>
    /// 将枚举值转换为显示字符串
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToDisplayString(this OfficialMessageType value) => value switch
    {
        OfficialMessageType.OfficialMessage => "官方公告",
        OfficialMessageType.Promotions => "优惠活动",
        OfficialMessageType.News => "最新消息",
        _ => value.ToString(),
    };

    /// <summary>
    /// 将枚举值转换为兼容的组 <see cref="Guid"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Guid ToCompatNoticeGroupId(this OfficialMessageType value)
    {
        var id = new byte[16];
        id[^1] = (byte)value;
        return new Guid(id);
    }
}