namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;

public interface IWeChatApiOptions
{
    string AppId { get; }

    string AppSecret { get; }

    string AESKey { get; }

    string Token { get; }
}
