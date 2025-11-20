using AspNet.Security.OAuth.Alipay;
using Microsoft.AspNetCore.Authentication;
using System.Net.Security;

#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
namespace Microsoft.Extensions.DependencyInjection;

public static partial class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder ConfigureExternalLoginChannels(this AuthenticationBuilder builder, ConfigurationManager configuration)
    {
#if !EXCLUDE_EXTERNALLOGINCHANNEL_STEAM
        // Steam, Valve Corporation
        var steamAppKey = configuration["Authentication:Steam:ApplicationKey"];
        var disableBackchannelHttpHandler = configuration["Authentication:Steam:DisableBackchannelHttpHandler"];
        _ = bool.TryParse(disableBackchannelHttpHandler, out var disableBackchannelHttpHandlerB);
        if (!string.IsNullOrWhiteSpace(steamAppKey))
        {
            builder.AddSteam(options =>
            {
                //options.Authority = new("https://store.steampowered.com/openid/");
                options.ApplicationKey = steamAppKey;
                options.CorrelationCookie.IsEssential = true;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.BackchannelTimeout = TimeSpan.FromMinutes(2);
                if (disableBackchannelHttpHandlerB == false)
                {
                    options.BackchannelHttpHandler = new SteamCommunityReverseProxyHttpClientHandler();
                }
                //options.BackchannelHttpHandler = new HttpClientHandler
                //{
                //    ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
                //};
                //options.BackchannelHttpHandler = new SocketsHttpHandler
                //{
                //    SslOptions = new SslClientAuthenticationOptions
                //    {
                //        RemoteCertificateValidationCallback = delegate { return true; },
                //    }
                //};
            });
        }
#endif

#if !EXCLUDE_EXTERNALLOGINCHANNEL_QQ
        // 腾讯 QQ
        var qqClientId = configuration["Authentication:QQ:ClientId"];
        var qqClientSecret = configuration["Authentication:QQ:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(qqClientId) && !string.IsNullOrWhiteSpace(qqClientSecret))
        {
            builder.AddQQ(options =>
            {
                options.ApplyForUnionId = true;
                options.ClientId = qqClientId;
                options.ClientSecret = qqClientSecret;
                options.BackchannelHttpHandler = new SocketsHttpHandler
                {
                    SslOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = delegate { return true; },
                    },
                };
            });
        }
#endif

#if !EXCLUDE_EXTERNALLOGINCHANNEL_MS
        // 微软账号
        var msClientId = configuration["Authentication:Microsoft:ClientId"];
        var msClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
        var msAuthEndpoint = configuration["Authentication:Microsoft:AuthorizationEndpoint"];
        var msTokenEndpoint = configuration["Authentication:Microsoft:TokenEndpoint"];
        if (!string.IsNullOrWhiteSpace(msClientId) && !string.IsNullOrWhiteSpace(msClientSecret) &&
            !string.IsNullOrWhiteSpace(msAuthEndpoint) && !string.IsNullOrWhiteSpace(msTokenEndpoint))
        {
            builder.AddMicrosoftAccount(options =>
            {
                options.ClientId = msClientId;
                options.ClientSecret = msClientSecret;
                options.AuthorizationEndpoint = msAuthEndpoint;
                options.TokenEndpoint = msTokenEndpoint;
            });
        }
#endif

#if !EXCLUDE_EXTERNALLOGINCHANNEL_ALIPAY
        // 支付宝
        var alipayOAuthOptions = configuration.GetSection("Authentication:Alipay");
        if (alipayOAuthOptions.Exists())
        {
            builder.AddAlipay2()
                .Services
                .AddOptions<Alipay2AuthenticationOptions>(AlipayAuthenticationDefaults.AuthenticationScheme)
            .Configure<IConfiguration, IServiceProvider>((options, configuration, serviceProvider) =>
            {
                alipayOAuthOptions.Bind(options);
                if (options.EnableCertSignature)
                {
                    // Otherwise assume the private key is stored locally on disk
                    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

                    options.UsePrivateKey(
                        keyId =>
                            environment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.pem"));
                }
            });
        }
#endif

#if !EXCLUDE_EXTERNALLOGINCHANNEL_WECHAT
        // 微信
        var weixinOAuthOptions = configuration.GetSection("Authentication:Weixin");
        if (weixinOAuthOptions.Exists())
        {
            builder.AddWeixin(options =>
            {
                weixinOAuthOptions.Bind(options);
            });
        }
#endif
        return builder;
    }
}
