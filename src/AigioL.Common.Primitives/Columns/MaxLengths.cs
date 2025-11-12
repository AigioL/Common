namespace AigioL.Common.Primitives.Columns;

/// <summary>
/// 最大长度
/// </summary>
public static partial class MaxLengths
{
    /// <summary>
    /// 颜色 16 进制值，#AARRGGBB
    /// </summary>
    public const int ColorHex = 9;

    /// <summary>
    /// 一般名称
    /// </summary>
    public const int Name = 20;

    /// <summary>
    /// 菜单 Key
    /// </summary>
    public const int MenuKey = 100;

    /// <summary>
    /// 菜单名称
    /// </summary>
    public const int MenuName = 50;

    /// <summary>
    /// 一般长名称
    /// </summary>
    public const int LongName = 400;

    /// <summary>
    /// 昵称
    /// </summary>
    public const int NickName = 20;

    /// <summary>
    /// 图标名称
    /// </summary>
    public const int IconName = 20;

    /// <summary>
    /// 短信验证码
    /// </summary>
    public const int SMS_CAPTCHA = 6;

    /// <summary>
    /// 用户名(用户名不是昵称，通常为唯一键，因可用用户名进行登录)
    /// </summary>
    public const int UserName = 128;

    /// <summary>
    /// Url 地址
    /// </summary>
    public const int Url = 2048;

    /// <summary>
    /// 一般文本字符串
    /// </summary>
    public const int Text = 1000;

    /// <summary>
    /// 文件扩展名，例如 .exe/.dll
    /// </summary>
    public const int FileExtension = 16;

    /// <summary>
    /// 微信 OpenId
    /// </summary>
    public const int WeChatId = 128;

    /// <summary>
    /// 微信 UnionId
    /// </summary>
    public const int WeChatUnionId = 192;

    /// <summary>
    /// 邮箱地址
    /// </summary>
    public const int Email = 256;

    /// <summary>
    /// 现实地址/收货地址
    /// </summary>
    public const int RealityAddress = 600;

    /// <summary>
    /// 标题
    /// </summary>
    public const int Title = 30;

    /// <summary>
    /// 长标题
    /// </summary>
    public const int LongTitle = 200;

    /// <summary>
    /// 哈希密码最大长度
    /// </summary>
    public const int PasswordHash = 256;

    /// <summary>
    /// <see cref="global::System.Guid"/> 的字符串格式最大长度
    /// </summary>
    public const int Guid = 38;

    /// <summary>
    /// Crc32 算法的哈希长度
    /// </summary>
    public const int Crc32 = 8;
}

static partial class MaxLengths // 业务相关字段
{
    /// <summary>
    /// 客户端用户昵称最大长度
    /// </summary>
    public const int CUserNickName = 36;

    /// <summary>
    /// 客户端用户个性签名最大长度
    /// </summary>
    public const int CUserPersonalizedSignature = 100;

    /// <summary>
    /// 客户端用户设备名称最大长度
    /// </summary>
    public const int CUserDeviceName = 100;

    /// <summary>
    /// 设备 Id，版本代号为 R
    /// </summary>
    public const int DeviceIdR = 7;

    /// <summary>
    /// SHA1 算法的哈希 Hex 字符串长度
    /// </summary>
    public const int SHA1 = 40;

    /// <summary>
    /// SHA256 算法的哈希 Hex 字符串长度
    /// </summary>
    public const int SHA256 = 64;

    /// <summary>
    /// SHA384 算法的哈希 Hex 字符串长度
    /// </summary>
    public const int SHA384 = 96;

    /// <summary>
    /// 设备 Id 最大长度
    /// </summary>
    public const int DeviceId = ShortGuid.StringLength + DeviceIdR + SHA256;

    /// <summary>
    /// 变更原因最大长度
    /// </summary>
    public const int ChangeReason = 600;

    /// <summary>
    /// IP 地址最大长度，通常 IPv6 的最大长度为 46，但可能有多个 IP 地址使用分号分割，例如 HTTP_X_FORWARDED_FOR
    /// </summary>
    public const int IPAddress = 540;

    /// <summary>
    /// 版本号最大长度
    /// </summary>
    public const int Version = 23;

    /// <summary>
    /// 版本信息最大长度
    /// </summary>
    public const int InformationalVersion = 32;

    /// <summary>
    /// 人类最长寿命(岁)
    /// </summary>
    public const int HumanAge = 122;

    /// <summary>
    /// 文件名称最大长度
    /// </summary>
    public const int FileName = 260;

    /// <summary>
    /// 文章标题最大长度
    /// </summary>
    public const int ArticleTitle = 200;

    public const int ArticleCategoryName = 300;

    public const int ArticleTagName = 30;

    /// <summary>
    /// 优惠劵名称最大长度
    /// </summary>
    public const int CouponName = 120;

    /// <summary>
    /// 订单号最大长度
    /// </summary>
    public const int OrderId = 64;

    /// <summary>
    /// 控制器名称最大长度限制
    /// </summary>
    public const int BMControllerName = 128;
}