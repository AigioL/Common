/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
 * for more information concerning the license and the contributors participating to this project.
 */

namespace AspNet.Security.OAuth.Alipay;

/// <summary>
/// Contains constants specific to the <see cref="Alipay2AuthenticationHandler"/>.
/// </summary>
public static class Alipay2AuthenticationConstants
{
    public static class Claims
    {
        public const string Avatar = AlipayAuthenticationConstants.Claims.Avatar;

        public const string Province = AlipayAuthenticationConstants.Claims.Province;

        public const string City = AlipayAuthenticationConstants.Claims.City;

        public const string Nickname = AlipayAuthenticationConstants.Claims.Nickname;

        /// <summary>
        /// The user's gender. F: Female; M: Male.
        /// </summary>
        public const string Gender = AlipayAuthenticationConstants.Claims.Gender;

        /// <summary>
        /// OpenID is the unique identifier of Alipay users in the application dimension.
        /// See https://opendocs.alipay.com/mini/0ai2i6
        /// </summary>
        public const string OpenId = "urn:alipay:open_id";

        /// <summary>
        /// Alipay user system internal identifier, will no longer be independently open in the future, and will be replaced by OpenID.
        /// </summary>
        public const string UserId = "urn:alipay:user_id";
    }
}
