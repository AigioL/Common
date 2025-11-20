/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
 * for more information concerning the license and the contributors participating to this project.
 */

using AspNet.Security.OAuth.Alipay;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to add Alipay authentication capabilities to an HTTP application pipeline.
/// </summary>
public static class Alipay2AuthenticationExtensions
{
    /// <summary>
    /// Adds <see cref="Alipay2AuthenticationHandler"/> to the specified
    /// <see cref="AuthenticationBuilder"/>, which enables Alipay authentication capabilities.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
    public static AuthenticationBuilder AddAlipay2(this AuthenticationBuilder builder)
    {
        return builder.AddAlipay2(AlipayAuthenticationDefaults.AuthenticationScheme, options => { });
    }

    /// <summary>
    /// Adds <see cref="Alipay2AuthenticationHandler"/> to the specified
    /// <see cref="AuthenticationBuilder"/>, which enables Alipay authentication capabilities.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="configuration">The delegate used to configure the OpenID 2.0 options.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
    public static AuthenticationBuilder AddAlipay2(
        this AuthenticationBuilder builder,
        Action<Alipay2AuthenticationOptions> configuration)
    {
        return builder.AddAlipay2(AlipayAuthenticationDefaults.AuthenticationScheme, configuration);
    }

    /// <summary>
    /// Adds <see cref="Alipay2AuthenticationHandler"/> to the specified
    /// <see cref="AuthenticationBuilder"/>, which enables Alipay authentication capabilities.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="scheme">The authentication scheme associated with this instance.</param>
    /// <param name="configuration">The delegate used to configure the Alipay options.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
    public static AuthenticationBuilder AddAlipay2(
        this AuthenticationBuilder builder,
        string scheme,
        Action<Alipay2AuthenticationOptions> configuration)
    {
        return builder.AddAlipay2(scheme, AlipayAuthenticationDefaults.DisplayName, configuration);
    }

    /// <summary>
    /// Adds <see cref="Alipay2AuthenticationHandler"/> to the specified
    /// <see cref="AuthenticationBuilder"/>, which enables Alipay authentication capabilities.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="scheme">The authentication scheme associated with this instance.</param>
    /// <param name="caption">The optional display name associated with this instance.</param>
    /// <param name="configuration">The delegate used to configure the Alipay options.</param>
    /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
    public static AuthenticationBuilder AddAlipay2(
        this AuthenticationBuilder builder,
        string scheme,
        string caption,
        Action<Alipay2AuthenticationOptions> configuration)
    {
        return builder.AddOAuth<Alipay2AuthenticationOptions, Alipay2AuthenticationHandler>(scheme, caption, configuration);
    }
}
