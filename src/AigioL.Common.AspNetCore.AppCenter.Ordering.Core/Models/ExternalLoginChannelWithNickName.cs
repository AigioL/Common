using AigioL.Common.AspNetCore.AppCenter.Models;

namespace AigioL.Common.AspNetCore.AppCenter.Ordering.Models;

/// <summary>
/// 第三方外部登录渠道与昵称
/// </summary>
/// <param name="Channel"></param>
/// <param name="NickName"></param>
public sealed record class ExternalLoginChannelWithNickName(ExternalLoginChannel Channel, string NickName)
{
}
