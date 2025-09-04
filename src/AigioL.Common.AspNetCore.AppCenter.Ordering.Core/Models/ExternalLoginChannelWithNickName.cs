using AigioL.Common.AspNetCore.AppCenter.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

public sealed record class ExternalLoginChannelWithNickName(ExternalLoginChannel Channel, string NickName)
{
}
