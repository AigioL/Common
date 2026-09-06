namespace AigioL.Common.Stm.Constants;

/// <summary>
/// Steam API 的 URL 常量类
/// </summary>
public static partial class SteamApiUrls
{
    public const string HttpClientName = "SteamCommunityProxy";

    /// <summary>
    /// Steam WebApi 站点使用的默认语言代码（简体中文）
    /// </summary>
    public const string DefaultLanguage = "zh";

    /// <summary>
    /// Steam 客户端、内嵌网页使用的默认语言代码（简体中文）
    /// </summary>
    public const string DefaultLanguage_Client = "schinese";

    /// <summary>
    /// Steam 社区域名
    /// </summary>
    public const string Domain_SteamCommunity = "steamcommunity.com";

    /// <summary>
    /// Steam WebApi 域名
    /// </summary>
    public const string Domain_SteamWebApi = "api.steampowered.com";

    /// <summary>
    /// Steam 登录 API 域名
    /// </summary>
    public const string Domain_SteamLoginApi = "login.steampowered.com";

    /// <summary>
    /// Steam 发行商、合作伙伴 API 域名
    /// </summary>
    public const string Domain_SteamPartnerApi = "partner.steam-api.com";

    /// <summary>
    /// Steam 社区基地址
    /// </summary>
    public const string Url_SteamCommunity = $"https://{Domain_SteamCommunity}";

    /// <summary>
    /// Steam WebApi 基地址
    /// </summary>
    public const string Url_SteamWebApi = $"https://{Domain_SteamWebApi}";

    /// <summary>
    /// Steam 登录 API 基地址
    /// </summary>
    public const string Url_SteamLoginApi = $"https://{Domain_SteamLoginApi}";

    /// <summary>
    /// Steam 发行商、合作伙伴 API 基地址
    /// </summary>
    public const string Url_SteamPartnerApi = $"https://{Domain_SteamPartnerApi}";

    public const string IDevService_GetSteamApiKey =
        $"{Url_SteamCommunity}/dev/apikey";

    public const string ISteamUser_GetPlayerBans_3 =
        $"{Url_SteamWebApi}/ISteamUser/GetPlayerBans/v1/?access_token={{0}}&steamids={{1}}&key={{2}}";

    public const string ISteamUser_GetPlayerSummaries_3 =
        $"{Url_SteamWebApi}/ISteamUser/GetPlayerSummaries/v1/?access_token={{0}}&steamids={{1}}&key={{2}}";

    public const string IInventoryService_GetMyInventory_5 =
        $"{Url_SteamCommunity}/inventory/{{0}}/{{1}}/{{2}}?l={{3}}&count={{4}}";

    public const string ISteamEconomy_GetAssetClassInfo_5 =
        $"{Url_SteamWebApi}/ISteamEconomy/GetAssetClassInfo/v1/?access_token={{0}}&appid={{1}}&language={{2}}&class_count={{3}}&classid0={{4}}";

    public const string IEconService_GetTradeHistory_4 =
        $"{Url_SteamWebApi}/IEconService/GetTradeHistory/v1/?access_token={{0}}&max_trades={{1}}&get_descriptions={{2}}&language={{3}}";

    public const string IEconService_GetTradeOffer_4 =
        $"{Url_SteamWebApi}/IEconService/GetTradeOffer/v1/?access_token={{0}}&tradeofferid={{1}}&language={{2}}&get_descriptions={{3}}";

    public const string IEconService_GetTradeOffers_4 =
        $"{Url_SteamWebApi}/IEconService/GetTradeOffers/v1/?access_token={{0}}&language={{1}}&get_descriptions={{2}}";

    public const string IEconService_GetTradeOfferAccessUrl_1 =
        $"{Url_SteamCommunity}/profiles/{{0}}/tradeoffers/privacy";

    public const string ISteamTradeService_NewSend =
        $"{Url_SteamCommunity}/tradeoffer/new/send";

    public const string ISteamTradeService_Cancel_1 =
        $"{Url_SteamCommunity}/tradeoffer/{{0}}/cancel";

    public const string ISteamTradeService_Decline_1 =
        $"{Url_SteamCommunity}/tradeoffer/{{0}}/decline";

    public const string ISteamTradeService_Accept_1 =
        $"{Url_SteamCommunity}/tradeoffer/{{0}}/accept";

    public const string ISteamTradeService_Receipt_1 =
        $"{Url_SteamCommunity}/trade/{{0}}/receipt";

    public const string IEconMarketService_GetMarketEligibility_2 =
        $"{Url_SteamPartnerApi}/IEconMarketService/GetMarketEligibility/v1/?key={{0}}&steamid={{1}}";

    public const string IEconMarketService_WebTradeEligibility_302 =
        $"{Url_SteamCommunity}/market/eligibilitycheck";

    public const string JwtAjaxRefresh =
        $"{Url_SteamLoginApi}/jwt/ajaxrefresh";

    public const string SetToken =
        $"{Url_SteamCommunity}/login/settoken";
}