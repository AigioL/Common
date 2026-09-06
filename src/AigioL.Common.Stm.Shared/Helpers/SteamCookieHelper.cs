using Microsoft.IdentityModel.JsonWebTokens;
using static AigioL.Common.Stm.Constants.SteamCookieConstants;

namespace AigioL.Common.Stm.Helpers;

/// <summary>
/// Steam Cookie 的处理帮助类
/// </summary>
public static partial class SteamCookieHelper
{
    /// <summary>
    /// 判断 <see cref="JsonWebToken"/> 是否在有效期内
    /// </summary>
    public static bool Verify(JsonWebToken? jwt, DateTimeOffset? now = null)
    {
        if (jwt != null)
        {
            // 这里 jwt 的时间为 utc，不要使用 DateTime，应使用 DateTimeOffset 比较
            now ??= DateTimeOffset.Now;
            if (now.Value < jwt.ValidTo && now.Value > jwt.ValidFrom)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断 <see cref="JsonWebToken"/> 是否包含 Steam 社区受众
    /// </summary>
    public static bool HasAudienceSteamCommunity(JsonWebToken? jwt)
    {
        if (jwt != null && jwt.Audiences != null)
        {
            return jwt.Audiences.Any(static x => x == JwtAudienceSteamCommunity);
        }

        return false;
    }

    /// <summary>
    /// 将 Cookie 值解析为 Steam64Id 与 <see cref="JsonWebToken"/>
    /// </summary>
    public static (ulong steam64Id, JsonWebToken jwt, ReadOnlyMemory<char> jwtString)? Parse(ReadOnlyMemory<char> chars)
    {
        ulong steam64Id = default;
        JsonWebToken? jwt;

        var split = chars.Span.Split(Separator);

        int i = 0;
        while (split.MoveNext())
        {
            var it = chars[split.Current];
            switch (i)
            {
                case 0:
                    steam64Id = ulong.Parse(it.Span);
                    break;
                case 1:
                    jwt = new JsonWebToken(it);
                    return (steam64Id, jwt, it);
            }

            i++;
        }

        return null;
    }
}
