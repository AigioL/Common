namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;

public interface IAppSettings : IPaySettings
{
    IWeChatApiOptions WeChatApiOptions { get; }
}
