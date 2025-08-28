using MemoryPack;

namespace AigioL.Common.AspNetCore.AppCenter.Models;

[MemoryPackable]
public readonly partial record struct UserDeviceIsTrustMap(Guid UserId, bool UserDeviceIsTrust)
{
}

#if DEBUG
[Obsolete("use UserIsBanMap", true)]
public partial record UserJsonWebTokenInfo(Guid UserId, bool IsTrust);
#endif