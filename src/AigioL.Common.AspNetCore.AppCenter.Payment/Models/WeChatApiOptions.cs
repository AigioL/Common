using AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;

namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models;

public sealed partial class WeChatApiOptions : IWeChatApiOptions
{
    public string AppId
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }

    public string AppSecret
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }

    public string AESKey
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }

    public string Token
    {
        get
        {
            ArgumentNullException.ThrowIfNull(field);
            return field;
        }
        set;
    }
}