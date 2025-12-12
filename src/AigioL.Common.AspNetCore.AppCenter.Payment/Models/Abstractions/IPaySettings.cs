namespace AigioL.Common.AspNetCore.AppCenter.Payment.Models.Abstractions;

public interface IPaySettings
{
    bool DebugOnlinePayment { get; }

    string OfficialUrl { get; }
}
