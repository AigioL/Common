namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;

public interface IAppSettings
{
    IWeChatApiOptions WeChatApiOptions { get; }
}
