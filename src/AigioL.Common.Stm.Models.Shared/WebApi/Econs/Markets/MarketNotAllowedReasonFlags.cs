namespace AigioL.Common.Stm.Models.WebApi.Econs.Markets;

/// <summary>
/// 市场封禁的原因标志位
/// <para>https://docs.rs/steamworks-sys/latest/steamworks_sys/struct.EMarketNotAllowedReasonFlags.html</para>
/// </summary>
[Flags]
public enum MarketNotAllowedReasonFlags
{
    /// <summary>
    /// 无封禁
    /// <para>0</para>
    /// </summary>
    None = 0,

    /// <summary>
    /// 后端调用失败，或某些操作可能在重试后再次生效
    /// <para>1</para>
    /// </summary>
    TemporaryFailure = 1 << 0,

    /// <summary>
    /// 账号被禁用
    /// <para>2</para>
    /// </summary>
    AccountDisabled = 1 << 1,

    /// <summary>
    /// 账号被锁定
    /// <para>4</para>
    /// </summary>
    AccountLockedDown = 1 << 2,

    /// <summary>
    /// 受限账户（不可购买）
    /// <para>8</para>
    /// </summary>
    AccountLimited = 1 << 3,

    /// <summary>
    ///  该账号被禁止交易物品
    /// <para>16</para>
    /// </summary>
    TradeBanned = 1 << 4,

    /// <summary>
    /// 钱包资金不可交易，因为用户在过去一年内未进行过购买活动，或在上个月之前未进行过任何购买
    /// <para>32</para>
    /// </summary>
    AccountNotTrusted = 1 << 5,

    /// <summary>
    /// 该用户未启用 Steam Guard 安全防护功能
    /// <para>64</para>
    /// </summary>
    SteamGuardNotEnabled = 1 << 6,

    /// <summary>
    /// 该用户已启用 Steam Guard 安全防护功能，但未达到所需的有效天数要求
    /// <para>128</para>
    /// </summary>
    SteamGuardOnlyRecentlyEnabled = 1 << 7,

    /// <summary>
    ///  用户最近忘记了密码并进行了重置
    ///  <para>256</para>
    /// </summary>
    RecentPasswordReset = 1 << 8,

    /// <summary>
    /// 用户最近使用新的支付方式为其钱包充值
    /// <para>512</para>
    /// </summary>
    NewPaymentMethod = 1 << 9,

    /// <summary>
    /// 用户发送了无效的 Cookie
    /// <para>1024</para>
    /// </summary>
    InvalidCookie = 1 << 10,

    /// <summary>
    /// 用户已启用 Steam Guard 安全防护，但正在使用新电脑或新网页浏览器
    /// <para>2048</para>
    /// </summary>
    UsingNewDevice = 1 << 11,

    /// <summary>
    /// 用户最近自行退还了商店购买的商品
    /// <para>4096</para>
    /// </summary>
    RecentSelfRefund = 1 << 12,

    /// <summary>
    /// 用户最近使用一种无法验证的新支付方式为其钱包充值
    /// <para>8192</para>
    /// </summary>
    NewPaymentMethodCannotBeVerified = 1 << 13,

    /// <summary>
    /// 该账户不仅不可信，而且近期没有任何购买记录
    /// <para>16384</para>
    /// </summary>
    NoRecentPurchases = 1 << 14,

    /// <summary>
    /// 用户接受了最近购买的钱包礼物
    /// <para>32768</para>
    /// </summary>
    AcceptedWalletGift = 1 << 15,

    /// <summary>
    /// 交易报价撤回冷却中
    /// <para>65536</para>
    /// </summary>
    TradeCooldown = 1 << 16,
}