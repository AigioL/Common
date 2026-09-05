using System.Diagnostics.CodeAnalysis;

namespace AigioL.Common.Stm.Models.WebApi.SteamUsers;

public sealed partial record RefreshAccessTokenResult
{
    public RefreshAccessTokenResultCode Code { get; init; }

    public string[]? SetCookies { get; init; }

    public bool TryGetSetCookies([NotNullWhen(true)] out string[]? setCookies, out RefreshAccessTokenResultCode code)
    {
        setCookies = SetCookies;
        code = Code;
        switch (code)
        {
            case RefreshAccessTokenResultCode.Success:
                {
                    if (setCookies == null || setCookies.Length == 0)
                    {
                        code = RefreshAccessTokenResultCode.ResponseSetCookiesIsNull;
                        return false;
                    }
                    return true;
                }
        }
        return false;
    }

    public static implicit operator RefreshAccessTokenResult(RefreshAccessTokenResultCode code) => new() { Code = code, };

    public static implicit operator RefreshAccessTokenResult(string[]? setCookies) => new()
    {
        Code = (setCookies == null || setCookies.Length == 0)
            ? RefreshAccessTokenResultCode.ResponseSetCookiesIsNull
            : RefreshAccessTokenResultCode.Success,
        SetCookies = setCookies,
    };
}
