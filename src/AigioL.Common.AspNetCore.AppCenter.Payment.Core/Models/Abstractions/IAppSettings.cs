namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;

public interface IAppSettings : IPaySettings, IWeChatApiAppSettings
{
}

public interface IWeChatApiAppSettings
{
    IWeChatApiOptions WeChatApiOptions { get; }
}